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
        public readonly ChessEvalMaterialBasic _evalMaterial;

        public readonly PhasedScore[][] _pcsqPiecePos = new PhasedScore[ChessPieceInfo.LookupArrayLength][];
        public readonly PhasedScore[][] _mobilityPieceTypeCount = new PhasedScore[ChessPieceTypeInfo.LookupArrayLength][];
        public readonly PhasedScore _matBishopPair;

        public readonly int[] _endgameMateKingPcSq;


        protected readonly ChessEvalSettings _settings;

        protected readonly PhasedScore RookFileOpen;
        protected readonly PhasedScore RookFileHalfOpen;

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
            _evalMaterial = new ChessEvalMaterialBasic(_settings);
            
            //bishop pairs
            _matBishopPair = PhasedScoreInfo.Create(settings.MaterialBishopPair.Opening,  settings.MaterialBishopPair.Endgame);


            //setup piecesq tables
            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
            {
                _pcsqPiecePos[(int)piece] = new PhasedScore[64];
                foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
                {
                
                    if (piece.PieceToPlayer() == ChessPlayer.White)
                    {
                        _pcsqPiecePos[(int)piece][(int)pos] = PhasedScoreInfo.Create(
                            settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos],
                            settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Endgame][pos]);
                    }
                    else
                    {
                        _pcsqPiecePos[(int)piece][(int)pos] = PhasedScoreInfo.Create(
                            -settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos.Reverse()],
                            -settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Endgame][pos.Reverse()]);
                    }
                    
                
                }
            }



            //setup mobility arrays

            foreach (ChessPieceType pieceType in ChessPieceTypeInfo.AllPieceTypes)
            {
                _mobilityPieceTypeCount[(int)pieceType] = new PhasedScore[28];
                for (int attacksCount = 0; attacksCount < 28; attacksCount++)
                {
                    var mob = settings.Mobility;
                    var opiece = mob[pieceType];

                    int startVal = (attacksCount - opiece[ChessGameStage.Opening].ExpectedAttacksAvailable) * opiece[ChessGameStage.Opening].AmountPerAttackDefault;
                    int endVal = (attacksCount - opiece[ChessGameStage.Endgame].ExpectedAttacksAvailable) * opiece[ChessGameStage.Endgame].AmountPerAttackDefault;

                    _mobilityPieceTypeCount[(int)pieceType][attacksCount] = PhasedScoreInfo.Create(startVal, endVal);
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

            RookFileOpen = PhasedScoreInfo.Create(settings.RookFileOpen, settings.RookFileOpen / 2);
            RookFileHalfOpen = PhasedScoreInfo.Create(settings.RookFileOpen / 2, settings.RookFileOpen / 4);

        }

        public void PcSqValuesAdd(ChessPiece piece, ChessPosition pos, ref PhasedScore value)
        {
            value = value.Add(_pcsqPiecePos[(int)piece][(int)pos]);
        }
        public void PcSqValuesRemove(ChessPiece piece, ChessPosition pos, ref PhasedScore value)
        {
            value = value.Subtract(_pcsqPiecePos[(int)piece][(int)pos]);
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
            
            
            var attacksWhite = evalInfo.Attacks[(int)ChessPlayer.White];
            var attacksBlack = evalInfo.Attacks[(int)ChessPlayer.Black];

            
            attacksWhite.PawnEast = board[ChessPiece.WPawn].ShiftDirNE();
            attacksWhite.PawnWest = board[ChessPiece.WPawn].ShiftDirNW();
            attacksBlack.PawnEast = board[ChessPiece.BPawn].ShiftDirSE();
            attacksBlack.PawnWest = board[ChessPiece.BPawn].ShiftDirSW();

            attacksWhite.King = Attacks.KingAttacks(board.KingPosition(ChessPlayer.White));
            attacksBlack.King = Attacks.KingAttacks(board.KingPosition(ChessPlayer.Black));


            EvaluateMyPieces(board, ChessPlayer.White, evalInfo);
            EvaluateMyPieces(board, ChessPlayer.Black, evalInfo);
           

            //material
            var material = _evalMaterial.EvalMaterialHash(board);

            //pawns
            PawnInfo pawns = this._evalPawns.PawnEval(board);            

            //eval passed pawns;
            this._evalPawns.EvalPassedPawns(board, evalInfo, pawns.PassedPawns);

            //shelter storm;
            if (material.DoShelter)
            {
                evalInfo.ShelterStorm = PhasedScoreInfo.Create(_settings.PawnShelterFactor * pawns.EvalShelter(
                    whiteKingFile: board.KingPosition(ChessPlayer.White).GetFile(),
                    blackKingFile: board.KingPosition(ChessPlayer.Black).GetFile(),
                    castleFlags: board.CastleRights), 0);
            }
            
            //evalInfo.ShelterStorm = this._evalPawns.EvalKingShelterStormBlackPerspective(board.KingPosition(ChessPlayer.White).GetFile(), board.PieceLocations(ChessPiece.WPawn), board.PieceLocations(ChessPiece.BPawn));
            //evalInfo.ShelterStorm -= this._evalPawns.EvalKingShelterStormBlackPerspective(, board.PieceLocations(ChessPiece.BPawn), board.PieceLocations(ChessPiece.WPawn));


            //get pcsq values from board.
            var valPieceSq = board.PcSqValue;

            //test to see if we are just trying to force the king to the corner for mate.
            PhasedScore endGamePcSq = 0;
            if (UseEndGamePcSq(board, ChessPlayer.White, out endGamePcSq))
            {
                valPieceSq = endGamePcSq;
                evalInfo.Attacks[0].Mobility = 0;
                evalInfo.Attacks[1].Mobility = 0;
            }
            else if (UseEndGamePcSq(board, ChessPlayer.Black, out endGamePcSq))
            {
                valPieceSq = endGamePcSq.Negate();
                evalInfo.Attacks[0].Mobility = 0;
                evalInfo.Attacks[1].Mobility = 0;
            }


            evalInfo.Material = material.Score;
            evalInfo.PcSqStart = valPieceSq;
            evalInfo.PawnsStart = pawns.Value;
            evalInfo.StageStartWeight = material.StartWeight;

            return evalInfo.Score;
        }

        protected int EvaluateMyPieces(ChessBoard board, ChessPlayer me, ChessEvalInfo info)
        {
            int retval = 0;
            PhasedScore mobility = 0;
            var myAttacks = info.Attacks[(int)me];

            ChessBitboard myPieces = board[me];
            ChessBitboard pieceLocationsAll = board.PieceLocationsAll;
            ChessBitboard pawns = board[ChessPieceType.Pawn];

            ChessBitboard slidersAndKnights = myPieces &
               (board[ChessPieceType.Knight]
               | board[ChessPieceType.Bishop]
               | board[ChessPieceType.Rook]
               | board[ChessPieceType.Queen]);


            while (slidersAndKnights != ChessBitboard.Empty) //foreach(ChessPosition pos in slidersAndKnights.ToPositions())
            {
                ChessPosition pos = ChessBitboardInfo.PopFirst(ref slidersAndKnights);

                ChessPieceType pieceType = board.PieceAt(pos).ToPieceType();

                //generate attacks
                ChessBitboard slidingAttacks = ChessBitboard.Empty;

                switch (pieceType)
                {
                    case ChessPieceType.Knight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        myAttacks.Knight |= slidingAttacks;
                        break;
                    case ChessPieceType.Bishop:
                        slidingAttacks = MagicBitboards.BishopAttacks(pos, pieceLocationsAll);
                        myAttacks.Bishop |= slidingAttacks;
                        break;
                    case ChessPieceType.Rook:
                        slidingAttacks = MagicBitboards.RookAttacks(pos, pieceLocationsAll);
                        myAttacks.RookQueen |= slidingAttacks;
                        if ((pos.GetFile().Bitboard() & pawns & myPieces) == ChessBitboard.Empty)
                        {
                            if ((pos.GetFile().Bitboard() & pawns) == ChessBitboard.Empty)
                            {
                                mobility = mobility.Add(RookFileOpen);
                            }
                            else
                            {
                                mobility = mobility.Add(RookFileHalfOpen);
                            }
                        }
                        break;
                    case ChessPieceType.Queen:
                        slidingAttacks = MagicBitboards.QueenAttacks(pos, pieceLocationsAll);
                        myAttacks.RookQueen |= slidingAttacks;
                        break;
                }
                //
                ChessBitboard slidingMoves = slidingAttacks & ~myPieces;
                int moveCount = slidingMoves.BitCount();
                mobility = mobility.Add(_mobilityPieceTypeCount[(int)pieceType][moveCount]);
            }

            myAttacks.Mobility = mobility;
            return retval;
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

        protected bool UseEndGamePcSq(ChessBoard board, ChessPlayer winPlayer, out PhasedScore newPcSq)
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
                    newPcSq = PhasedScoreInfo.Create(0, _endgameMateKingPcSq[(int)loseKing] - (winKing.DistanceTo(loseKing) * 25));
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

        public PhasedScore Mobility;

        public void Reset()
        {
            PawnEast = ChessBitboard.Empty;
            PawnWest = ChessBitboard.Empty;
            Knight = ChessBitboard.Empty;
            Bishop = ChessBitboard.Empty;
            RookQueen = ChessBitboard.Empty;
            King = ChessBitboard.Empty;
            Mobility = 0;
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
        
        public int Material = 0;

        public PhasedScore PcSqStart = 0;

        public PhasedScore PawnsStart = 0;

        public PhasedScore PawnsPassedStart = 0;

        public PhasedScore ShelterStorm = 0;
        public int StageStartWeight = 0;

        public void Reset()
        {
            Attacks[0].Reset();
            Attacks[1].Reset();
            Material = 0;
            PcSqStart = 0;
            PawnsStart = 0;
            PawnsPassedStart = 0;
            ShelterStorm = 0;
            StageStartWeight = 0;
        }

        public int StageEndWeight
        {
            get { return 100 - StageStartWeight; }
        }

        public int Score
        {
            get
            {
                return PcSqStart
                    .Add(PawnsStart)
                    .Add(PawnsPassedStart)
                    .Add(ShelterStorm)
                    .Add(this.Attacks[0].Mobility.Subtract(this.Attacks[1].Mobility)).ApplyWeights(StageStartWeight) + Material;
            }
        }

        public int PcSq
        {
            get
            {
                return PcSqStart.ApplyWeights(StageStartWeight);
            }
        }

        public int Mobility
        {
            get
            {
                return Attacks[0].Mobility.Subtract(Attacks[1].Mobility).ApplyWeights(StageStartWeight);
            }
        }
        public int Pawns
        {
            get
            {
                return PawnsStart.ApplyWeights(StageStartWeight);
            }
        }

        public int PawnsPassed
        {
            get
            {
                return PawnsPassedStart.ApplyWeights(StageStartWeight);
            }
        }


    }
}
