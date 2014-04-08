using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public interface IChessEval
    {
        int EvalFor(ChessBoard board, ChessPlayer who);
    }

    public class ChessEval: IChessEval
    {

        protected readonly ChessEvalPawns _evalPawns;
        public readonly ChessEvalMaterial _evalMaterial;

        public readonly int[,,] _pcsqPiecePosStage = new int[12,64,2];
        public readonly int[,] _matPieceStage = new int[12,2];
        public readonly int[, ,] _mobilityPiecesStage = new int[28, 12, 2];
        public readonly int[] _matBishopPairStage = new int[2];
        public readonly int[] _endgameMateKingPcSq;

        protected readonly int WeightMaterialOpening;
        protected readonly int WeightMaterialEndgame;
        protected readonly int WeightPcSqOpening;
        protected readonly int WeightPcSqEndgame;
        protected readonly int WeightMobilityOpening;
        protected readonly int WeightMobilityEndgame;

        protected readonly ChessEvalSettings _settings;


        public static readonly ChessEval Default = new ChessEval();

        public static int TotalEvalCount = 0;

        public ChessEval()
            : this(ChessEvalSettings.Default())
        {

        }

        public ChessEval(ChessEvalSettings settings)
        {
            _settings = settings.CloneDeep();

            //setup pawn evaluation
            _evalPawns = new ChessEvalPawns(_settings, 10000);
            _evalMaterial = new ChessEvalMaterial(_settings);

            //setup weight values;
            WeightMaterialOpening = settings.Weight.Material.Opening;
            WeightMaterialEndgame = settings.Weight.Material.Endgame;
            WeightPcSqOpening = settings.Weight.PcSq.Opening;
            WeightPcSqEndgame = settings.Weight.PcSq.Endgame;
            WeightMobilityOpening = settings.Weight.Mobility.Opening;
            WeightMobilityEndgame = settings.Weight.Mobility.Endgame;

            //bishop pairs
            _matBishopPairStage[(int)ChessGameStage.Opening] = settings.MaterialBishopPair.Opening;
            _matBishopPairStage[(int)ChessGameStage.Endgame] = settings.MaterialBishopPair.Endgame;

            //setup material arrays
            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
            {
                foreach (ChessGameStage stage in ChessGameStageInfo.AllGameStages)
                {
                    if (piece.PieceToPlayer() == ChessPlayer.White)
                    {
                        _matPieceStage[(int)piece, (int)stage] = settings.MaterialValues[piece.ToPieceType()][stage];
                    }
                    else
                    {
                        _matPieceStage[(int)piece, (int)stage] = -settings.MaterialValues[piece.ToPieceType()][stage];
                    }
                }
            }

            //setup piecesq tables
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
                {
                    foreach (ChessGameStage stage in ChessGameStageInfo.AllGameStages)
                    {
                        if (piece.PieceToPlayer() == ChessPlayer.White)
                        {
                            _pcsqPiecePosStage[(int)piece, (int)pos, (int)stage] = settings.PcSqTables[piece.ToPieceType()][stage][pos];
                        }
                        else
                        {
                            _pcsqPiecePosStage[(int)piece, (int)pos, (int)stage] = -settings.PcSqTables[piece.ToPieceType()][stage][pos.Reverse()];
                        }
                    }
                    
                }
            }
            



            //setup mobility arrays

            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
            {
                foreach (ChessGameStage stage in ChessGameStageInfo.AllGameStages)
                {
                    for (int attacksCount = 0; attacksCount < 28; attacksCount++)
                    {
                        var mob = settings.Mobility;
                        var opiece = mob[piece.ToPieceType()];
                        var ostage = opiece[stage];

                        int val = (attacksCount - ostage.ExpectedAttacksAvailable) * ostage.AmountPerAttackDefault;
                        _mobilityPiecesStage[attacksCount, (int)piece, (int)stage] = piece.PieceToPlayer() == ChessPlayer.White ? val : -val;
                    }
                    
                }
            }

            //initialize pcsq for trying to mate king in endgame, try to push it to edge of board.
            _endgameMateKingPcSq = new int[64];
            foreach (var pos in ChessPositionInfo.AllPositions)
            {
                List<int> distToMid = new List<int>();
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.D4));
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.D5));
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.E4));
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.E5));
                var minDist = distToMid.Min();
                _endgameMateKingPcSq[(int)pos] = minDist * 50;
            }
            
        }

        public void PcSqValuesAdd(ChessPiece piece, ChessPosition pos, ref int startValue, ref int endValue)
        {
            startValue += this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Opening];
            endValue += this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Endgame];
        }
        public void PcSqValuesRemove(ChessPiece piece, ChessPosition pos, ref int startValue, ref int endValue)
        {
            startValue -= this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Opening];
            endValue -= this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Endgame];
        }

        public int EvalFor(ChessBoard board, ChessPlayer who)
        {
            int retval = Eval(board);
            if (who == ChessPlayer.Black) { retval = -retval; }
            return retval;

        }

        private ChessEvalInfo _evalInfo = new ChessEvalInfo();
        public virtual int Eval(ChessBoard board)
        {
            return EvalDetail(board, _evalInfo);
        }


        public int EvalDetail(ChessBoard board, ChessEvalInfo evalInfo)
        {
            TotalEvalCount++;
            evalInfo.Reset();
            
            
            int valStartMobility = 0;
            int valEndMobility = 0;

            var attacksWhite = evalInfo.Attacks[(int)ChessPlayer.White];
            var attacksBlack = evalInfo.Attacks[(int)ChessPlayer.Black];

            
            attacksWhite.PawnEast = board.PieceLocations(ChessPiece.WPawn).ShiftDirNE();
            attacksWhite.PawnWest = board.PieceLocations(ChessPiece.WPawn).ShiftDirNW();
            attacksBlack.PawnEast = board.PieceLocations(ChessPiece.BPawn).ShiftDirSE();
            attacksBlack.PawnWest = board.PieceLocations(ChessPiece.BPawn).ShiftDirSW();

            attacksWhite.King = Attacks.KingAttacks(board.KingPosition(ChessPlayer.White));
            attacksBlack.King = Attacks.KingAttacks(board.KingPosition(ChessPlayer.Black));

            ChessBitboard slidersAndKnights = board.PieceLocationsAll 
                & ~(board.PieceLocations(ChessPiece.WPawn) 
                | board.PieceLocations(ChessPiece.BPawn)
                | board.PieceLocations(ChessPiece.WKing)
                | board.PieceLocations(ChessPiece.BKing)
                );

            foreach(ChessPosition pos in slidersAndKnights.ToPositions())
            {
                //ChessPosition pos = ChessPositionInfo.AllPositions[ipos];
                ChessPiece piece = board.PieceAt(pos);
                //if (piece == ChessPiece.EMPTY) { continue; }

                //generate attacks
                ChessBitboard slidingAttacks = ChessBitboard.Empty;
                
                switch (piece)
                {
                    case ChessPiece.WKnight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        attacksWhite.Knight |= slidingAttacks;
                        break;
                    case ChessPiece.WBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        attacksWhite.Bishop |= slidingAttacks;
                        break;
                    case ChessPiece.WRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        attacksWhite.RookQueen |= slidingAttacks;
                        break;
                    case ChessPiece.WQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        attacksWhite.RookQueen |= slidingAttacks;
                        break;
                    case ChessPiece.BKnight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        attacksBlack.Knight |= slidingAttacks;
                        break;
                    case ChessPiece.BBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        attacksBlack.Bishop |= slidingAttacks;
                        break;
                    case ChessPiece.BRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        attacksBlack.RookQueen |= slidingAttacks;
                        break;
                    case ChessPiece.BQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        attacksBlack.RookQueen |= slidingAttacks;
                        break;
                }
                //
                ChessBitboard slidingMoves = slidingAttacks & ~board.PlayerLocations(piece.PieceToPlayer());
                int moveCount = slidingMoves.BitCount();
                valStartMobility += _mobilityPiecesStage[moveCount, (int)piece, (int)ChessGameStage.Opening];
                valEndMobility += _mobilityPiecesStage[moveCount, (int)piece, (int)ChessGameStage.Endgame];
            }
            

            //material
            var material = _evalMaterial.EvalMaterialHash(board);

            //pawns
            PawnInfo pawns = this._evalPawns.PawnEval(board);            

            //eval passed pawns;
            this._evalPawns.EvalPassedPawns(board, evalInfo, pawns.PassedPawns);

            //shelter storm;
            if (material.DoShelter)
            {
                evalInfo.ShelterStorm = _settings.PawnShelterFactor * pawns.EvalShelter(
                    whiteKingFile: board.KingPosition(ChessPlayer.White).GetFile(),
                    blackKingFile: board.KingPosition(ChessPlayer.Black).GetFile(),
                    wsCastle: board.CastleAvailWS,
                    wlCastle: board.CastleAvailWL,
                    bsCastle: board.CastleAvailBS,
                    blCastle: board.CastleAvailBL);
            }
            
            //evalInfo.ShelterStorm = this._evalPawns.EvalKingShelterStormBlackPerspective(board.KingPosition(ChessPlayer.White).GetFile(), board.PieceLocations(ChessPiece.WPawn), board.PieceLocations(ChessPiece.BPawn));
            //evalInfo.ShelterStorm -= this._evalPawns.EvalKingShelterStormBlackPerspective(, board.PieceLocations(ChessPiece.BPawn), board.PieceLocations(ChessPiece.WPawn));


            //get pcsq values from board.
            int valStartPieceSq = board.PcSqValueStart;
            int valEndPieceSq = board.PcSqValueEnd;

            //test to see if we are just trying to force the king to the corner for mate.
            int endGamePcSq = 0;
            if (UseEndGamePcSq(board, ChessPlayer.White, out endGamePcSq))
            {
                valEndPieceSq = endGamePcSq;
                valEndMobility = 0;
            }
            else if (UseEndGamePcSq(board, ChessPlayer.Black, out endGamePcSq))
            {
                valEndPieceSq = -endGamePcSq;
                valEndMobility = 0;
            }


            evalInfo.MatStart = material.ScoreStart;
            evalInfo.MatEnd = material.ScoreEnd;
            evalInfo.PcSqStart = valStartPieceSq;
            evalInfo.PcSqEnd = valEndPieceSq;
            evalInfo.MobStart = valStartMobility;
            evalInfo.MobEnd = valEndMobility;
            evalInfo.PawnsStart = pawns.StartVal;
            evalInfo.PawnsEnd = pawns.EndVal;
            evalInfo.StageStartWeight = material.StartWeight;

            return evalInfo.Score;
        }

        protected virtual float CalcStartWeight(int basicMaterialCount)
        {
            //full material would be 62
            if (basicMaterialCount >= 56)
            {
                return 1;
            }
            else if (basicMaterialCount <= 10)
            {
                return 0;
            }
            else
            {
                int rem = basicMaterialCount - 10;
                float retval = (float)rem / 46f;
                return retval;
            }
        }

        protected bool UseEndGamePcSq(ChessBoard board, ChessPlayer winPlayer, out int newPcSq)
        {
            ChessPlayer losePlayer = winPlayer.PlayerOther();
            if (
                board.PieceCount(losePlayer, ChessPieceType.Pawn) == 0
                && board.PieceCount(losePlayer, ChessPieceType.Queen) == 0
                && board.PieceCount(losePlayer, ChessPieceType.Rook) == 0
                && (board.PieceCount(losePlayer, ChessPieceType.Bishop) + board.PieceCount(losePlayer, ChessPieceType.Knight) <= 1))
            {
                if(board.PieceCount(winPlayer, ChessPieceType.Queen) > 0
                    || board.PieceCount(winPlayer, ChessPieceType.Rook) > 0
                    || board.PieceCount(winPlayer, ChessPieceType.Bishop) + board.PieceCount(winPlayer, ChessPieceType.Bishop) >= 2)
                {
                    ChessPosition loseKing = board.KingPosition(losePlayer);
                    ChessPosition winKing = board.KingPosition(winPlayer);
                    newPcSq = _endgameMateKingPcSq[(int)loseKing] - (winKing.DistanceTo(loseKing) * 25);
                    return true;
                }
            }
            newPcSq = 0;
            return false;
        }

        
    }

    
    public class ChessEvalAttackInfo
    {
        public ChessBitboard PawnEast;
        public ChessBitboard PawnWest;

        public ChessBitboard Knight;
        public ChessBitboard Bishop;

        public ChessBitboard RookQueen;

        public ChessBitboard King;

        public void Reset()
        {
            PawnEast = ChessBitboard.Empty;
            PawnWest = ChessBitboard.Empty;
            Knight = ChessBitboard.Empty;
            Bishop = ChessBitboard.Empty;
            RookQueen = ChessBitboard.Empty;
            King = ChessBitboard.Empty;
        }

        public ChessBitboard All()
        {
            return PawnEast | PawnWest | Knight | Bishop | RookQueen | King;
        }

        public ChessEvalAttackInfo Reverse()
        {
            return new ChessEvalAttackInfo()
            {
                PawnEast = PawnEast.Reverse(),
                PawnWest = PawnWest.Reverse(),
                Knight = Knight.Reverse(),
                Bishop = Bishop.Reverse(),
                RookQueen = RookQueen.Reverse(),
                King = King.Reverse()
            };
        }
    }

    public class ChessEvalInfo
    {
        public ChessEvalAttackInfo[] Attacks = new ChessEvalAttackInfo[] { new ChessEvalAttackInfo(), new ChessEvalAttackInfo() };
        public int MatStart = 0;
        public int MatEnd = 0;
        public int PcSqStart = 0;
        public int PcSqEnd = 0;
        public int MobStart = 0;
        public int MobEnd = 0;
        public int PawnsStart = 0;
        public int PawnsEnd = 0;
        public int PawnsPassedStart = 0;
        public int PawnsPassedEnd = 0;
        public int ShelterStorm = 0;
        public float StageStartWeight = 0;

        public void Reset()
        {
            Attacks[0].Reset();
            Attacks[1].Reset();
            MatStart = 0;
            MatEnd = 0;
            PcSqStart = 0;
            PcSqEnd = 0;
            MobStart = 0;
            MobEnd = 0;
            PawnsStart = 0;
            PawnsEnd = 0;
            PawnsPassedStart = 0;
            PawnsPassedEnd = 0;
            ShelterStorm = 0;
            StageStartWeight = 0;
        }

        public float StageEndWeight
        {
            get { return 1 - StageStartWeight; }
        }

        public int ScoreStart
        {
            get
            {
                return MatStart + PcSqStart + MobStart + PawnsStart + PawnsPassedStart + ShelterStorm;
            }
        }
        public int ScoreEnd
        {
            get
            {
                return MatEnd + PcSqEnd + MobEnd + PawnsEnd + PawnsPassedEnd;
            }
        }
        public int Score
        {
            get
            {
                return (int)(((float)ScoreStart * StageStartWeight) + ((float)ScoreEnd * StageEndWeight));
            }
        }

        public int Material
        {
            get
            {
                return (int)(((float)MatStart * StageStartWeight) + ((float)MatEnd * StageEndWeight));
            }
        }

        public int PcSq
        {
            get
            {
                return (int)(((float)PcSqStart * StageStartWeight) + ((float)PcSqEnd * StageEndWeight));
            }
        }

        public int Mobility
        {
            get
            {
                return (int)(((float)MobStart * StageStartWeight) + ((float)MobEnd * StageEndWeight));
            }
        }
        public int Pawns
        {
            get
            {
                return (int)(((float)PawnsStart * StageStartWeight) + ((float)PawnsEnd * StageEndWeight));
            }
        }

        public int PawnsPassed
        {
            get
            {
                return (int)(((float)PawnsPassedStart * StageStartWeight) + ((float)PawnsPassedEnd * StageEndWeight));
            }
        }
        
        
    }
}
