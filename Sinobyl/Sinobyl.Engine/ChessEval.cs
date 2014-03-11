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

        protected readonly ChessEvalPawns PawnEval;

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





        public ChessEval()
            : this(ChessEvalSettings.Default())
        {

        }

        public ChessEval(ChessEvalSettings settings)
        {
            //setup pawn evaluation
            PawnEval = new ChessEvalPawns(settings, 1000);

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
                            _pcsqPiecePosStage[(int)piece, (int)pos, (int)stage] = settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos];
                        }
                        else
                        {
                            _pcsqPiecePosStage[(int)piece, (int)pos, (int)stage] = -settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos.Reverse()];
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

        public int EvalFor(ChessBoard board, ChessPlayer who)
        {
            int retval = Eval(board);
            if (who == ChessPlayer.Black) { retval = -retval; }
            return retval;

        }

        public virtual int Eval(ChessBoard board)
        {
            var result = EvalDetail(board);
            return result.Score;
        }

        public virtual ChessEvalInfo EvalDetail(ChessBoard board)
        {
            ChessEvalInfo evalInfo = new ChessEvalInfo();
            

            int valStartMat = 0;
            int valEndMat = 0;
            int valStartPieceSq = 0;
            int valEndPieceSq = 0;
            int valStartMobility = 0;
            int valEndMobility = 0;
            int basicMaterialCount = 0;

            var attacksWhite = evalInfo.Attacks[(int)ChessPlayer.White];
            var attacksBlack = evalInfo.Attacks[(int)ChessPlayer.Black];

            attacksWhite.PawnEast = board.PieceLocations(ChessPiece.WPawn).ShiftDirNE();
            attacksWhite.PawnWest = board.PieceLocations(ChessPiece.WPawn).ShiftDirNW();
            attacksBlack.PawnEast = board.PieceLocations(ChessPiece.BPawn).ShiftDirSE();
            attacksBlack.PawnWest = board.PieceLocations(ChessPiece.BPawn).ShiftDirSW();

            for (int ipos = 0; ipos < 64; ipos++)
            {
                ChessPosition pos = ChessPositionInfo.AllPositions[ipos];
                ChessPiece piece = board.PieceAt(pos);
                if (piece == ChessPiece.EMPTY) { continue; }

                //add material score
                valStartMat += this._matPieceStage[(int)piece, (int)ChessGameStage.Opening];
                valEndMat += this._matPieceStage[(int)piece, (int)ChessGameStage.Endgame];

                //add pcsq score;
                valStartPieceSq += this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Opening];
                valEndPieceSq += this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Endgame];

                //generate attacks
                ChessBitboard slidingAttacks = ChessBitboard.Empty;
                
                switch (piece)
                {
                    case ChessPiece.WPawn:
                        break;
                    case ChessPiece.WKnight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        if (attacksWhite.Knight1.Empty()) { attacksWhite.Knight1 = slidingAttacks; } else { attacksWhite.Knight2 = slidingAttacks; }
                        basicMaterialCount += 3;
                        break;
                    case ChessPiece.WBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        if (attacksWhite.Bishop1.Empty()) { attacksWhite.Bishop1 = slidingAttacks; } else { attacksWhite.Bishop2 = slidingAttacks; }
                        basicMaterialCount += 3;
                        break;
                    case ChessPiece.WRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        if (attacksWhite.Rook1.Empty()) { attacksWhite.Rook1 = slidingAttacks; } else { attacksWhite.Rook2 = slidingAttacks; }
                        basicMaterialCount += 5;
                        break;
                    case ChessPiece.WQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        attacksWhite.Queen = slidingAttacks;
                        basicMaterialCount += 9;
                        break;
                    case ChessPiece.WKing:
                        break;
                    case ChessPiece.BPawn:
                        break;
                    case ChessPiece.BKnight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        if (attacksBlack.Knight1.Empty()) { attacksBlack.Knight1 = slidingAttacks; } else { attacksBlack.Knight2 = slidingAttacks; }
                        basicMaterialCount += 3;
                        break;
                    case ChessPiece.BBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        if (attacksBlack.Bishop1.Empty()) { attacksBlack.Bishop1 = slidingAttacks; } else { attacksBlack.Bishop2 = slidingAttacks; }
                        basicMaterialCount += 3;
                        break;
                    case ChessPiece.BRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        if (attacksBlack.Rook1.Empty()) { attacksBlack.Rook1 = slidingAttacks; } else { attacksBlack.Rook2 = slidingAttacks; }
                        basicMaterialCount += 5;
                        break;
                    case ChessPiece.BQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        attacksBlack.Queen = slidingAttacks;
                        basicMaterialCount += 9;
                        break;
                    case ChessPiece.BKing:
                        break;
                }
                //
                ChessBitboard slidingMoves = slidingAttacks & ~board.PlayerLocations(piece.PieceToPlayer());
                int moveCount = slidingMoves.BitCount();
                valStartMobility += _mobilityPiecesStage[moveCount, (int)piece, (int)ChessGameStage.Opening];
                valEndMobility += _mobilityPiecesStage[moveCount, (int)piece, (int)ChessGameStage.Endgame];
            }
            
            //bishop pairs
            if (board.PieceCount(ChessPiece.WBishop) > 1)
            {
                valStartMat += _matBishopPairStage[(int)ChessGameStage.Opening];
                valEndMat += _matBishopPairStage[(int)ChessGameStage.Endgame];
            }
            if (board.PieceCount(ChessPiece.BBishop) > 1)
            {
                valStartMat -= _matBishopPairStage[(int)ChessGameStage.Opening];
                valEndMat -= _matBishopPairStage[(int)ChessGameStage.Endgame];
            }

            //pawns
            PawnInfo pawns = this.PawnEval.PawnEval(board);


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


            //calculate total start and end values
            int valStart = 
                (valStartMat * WeightMaterialOpening / 100) 
                + (valStartPieceSq * WeightPcSqOpening / 100) 
                + (valStartMobility * WeightMobilityOpening / 100) 
                + pawns.StartVal;

            int valEnd =
                (valEndMat * WeightMaterialEndgame / 100)
                + (valEndPieceSq * WeightPcSqEndgame / 100)
                + (valEndMobility * WeightMobilityEndgame / 100)
                + pawns.EndVal;

            float startWeight = CalcStartWeight(basicMaterialCount);
            float endWeight = 1 - startWeight;

            
            evalInfo.MatStart = valStartMat;
            evalInfo.MatEnd = valEndMat;
            evalInfo.PcSqStart = valStartPieceSq;
            evalInfo.PcSqEnd = valEndPieceSq;
            evalInfo.MobStart = valStartMobility;
            evalInfo.MobEnd = valEndMobility;
            evalInfo.PawnsStart = pawns.StartVal;
            evalInfo.PawnsEnd = pawns.EndVal;
            evalInfo.StageStartWeight = startWeight;

            return evalInfo;

            //int retval = (int)(valStart * startWeight) + (int)(valEnd * endWeight);

            //return retval;
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

        public ChessBitboard Knight1;
        public ChessBitboard Knight2;

        public ChessBitboard Bishop1;
        public ChessBitboard Bishop2;

        public ChessBitboard Rook1;
        public ChessBitboard Rook2;

        public ChessBitboard Queen;
        public ChessBitboard King2;
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
        public float StageStartWeight = 0;

        public float StageEndWeight
        {
            get { return 1 - StageStartWeight; }
        }

        public int ScoreStart
        {
            get
            {
                return MatStart + PcSqStart + MobStart + PawnsStart + PawnsPassedStart;
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
