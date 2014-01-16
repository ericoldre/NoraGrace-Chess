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
                foreach (ChessGameStage stage in Chess.AllGameStages)
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
                    foreach (ChessGameStage stage in Chess.AllGameStages)
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
                foreach (ChessGameStage stage in Chess.AllGameStages)
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
            
        }

        public int EvalFor(ChessBoard board, ChessPlayer who)
        {
            int retval = Eval(board);
            if (who == ChessPlayer.Black) { retval = -retval; }
            return retval;

        }

        public virtual int Eval(ChessBoard board)
        {

            int valStartMat = 0;
            int valEndMat = 0;
            int valStartPieceSq = 0;
            int valEndPieceSq = 0;
            int valStartMobility = 0;
            int valEndMobility = 0;
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
                        break;
                    case ChessPiece.WBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        break;
                    case ChessPiece.WRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        break;
                    case ChessPiece.WQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        break;
                    case ChessPiece.WKing:
                        break;
                    case ChessPiece.BPawn:
                        break;
                    case ChessPiece.BKnight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        break;
                    case ChessPiece.BBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        break;
                    case ChessPiece.BRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        break;
                    case ChessPiece.BQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
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

            float startWeight;
            float endWeight;
            CalcStartEndWeights(board, out startWeight, out endWeight);

            int retval = (int)(valStart * startWeight) + (int)(valEnd * endWeight);

            return retval;
        }

        protected virtual void CalcStartEndWeights(ChessBoard board, out float startWeight, out float endWeight)
        {
            int WhiteCount = board.PieceCount(ChessPiece.WKnight) + board.PieceCount(ChessPiece.WBishop) + board.PieceCount(ChessPiece.WRook) + board.PieceCount(ChessPiece.WQueen);
            int BlackCount = board.PieceCount(ChessPiece.BKnight) + board.PieceCount(ChessPiece.BBishop) + board.PieceCount(ChessPiece.BRook) + board.PieceCount(ChessPiece.BQueen);

            startWeight = (float)(WhiteCount + BlackCount) / (float)14;
            endWeight = 1 - startWeight;

        }

    }
}
