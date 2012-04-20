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

        private readonly ChessEvalPawns PawnEval;

        public readonly int[,,] _pcsqPiecePosStage = new int[12,64,2];
        public readonly int[,] _matPieceStage = new int[12,2];
        public readonly int[, ,] _mobilityPiecesStage = new int[28, 12, 2];

        private readonly int WeightMaterialOpening;
        private readonly int WeightMaterialEndgame;
        private readonly int WeightPcSqOpening;
        private readonly int WeightPcSqEndgame;
        private readonly int WeightMobilityOpening;
        private readonly int WeightMobilityEndgame;


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

            //setup material arrays
            foreach (ChessPiece piece in Chess.AllPieces)
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
            foreach (ChessPosition pos in Chess.AllPositions)
            {
                foreach (ChessPiece piece in Chess.AllPieces)
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
            
            foreach (ChessPiece piece in Chess.AllPieces)
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
                ChessPosition pos = Chess.AllPositions[ipos];
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
                + pawns.StartVal;

            //calculate current stage of the game
            int WhiteCount = board.PieceCount(ChessPiece.WKnight) + board.PieceCount(ChessPiece.WBishop) + board.PieceCount(ChessPiece.WRook) + board.PieceCount(ChessPiece.WQueen);
            int BlackCount = board.PieceCount(ChessPiece.BKnight) + board.PieceCount(ChessPiece.BBishop) + board.PieceCount(ChessPiece.BRook) + board.PieceCount(ChessPiece.BQueen);
            
            float StartFactor = (float)(WhiteCount + BlackCount) / (float)14;
            float EndFactor = 1 - StartFactor;


            int retval = (int)(valStart * StartFactor) + (int)(valEnd * EndFactor);

            return retval;
        }

    }
}
