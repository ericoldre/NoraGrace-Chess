using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sinobyl.Engine
{
    public enum ChessNotationType
    {
        Coord,
        San,
        Detailed
    }

    public class ChessMoves : List<ChessMove>
    {
        public ChessMoves()
        {

        }
        public ChessMoves(IEnumerable<ChessMove> moves)
            : base(moves)
        {

        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ChessMove move in this)
            {
                sb.Append(move.Write() + " ");
            }
            return sb.ToString();
        }
        public string ToString(ChessBoard board, bool isVariation)
        {
            StringBuilder sb = new StringBuilder();
            long zobInit = board.Zobrist;
            foreach (ChessMove move in this)
            {
                if (isVariation && board.WhosTurn == ChessPlayer.White)
                {
                    sb.Append(board.FullMoveCount.ToString() + ". ");
                }
                sb.Append(move.Write(board) + " ");
                if (isVariation)
                {
                    board.MoveApply(move);
                }
            }
            if (isVariation)
            {
                foreach (ChessMove move in this)
                {
                    board.MoveUndo();
                }
            }
            return sb.ToString();
        }

    }


    public static partial class ChessMoveInfo
    {
        public static ChessMove Parse(ChessBoard board, string movetext)
        {
            ChessPiece Promote = ChessPiece.EMPTY;//unless changed below
            ChessPosition From = (ChessPosition.OUTOFBOUNDS);
            ChessPosition To = (ChessPosition.OUTOFBOUNDS);
            Regex regex = new Regex("");

            movetext = movetext.Replace("+", "");
            movetext = movetext.Replace("x", "");
            movetext = movetext.Replace("#", "");
            movetext = movetext.Replace("=", "");

            ChessPlayer me = board.WhosTurn;
            ChessPiece mypawn = board.WhosTurn == ChessPlayer.White ? ChessPiece.WPawn : ChessPiece.BPawn;
            ChessPiece myknight = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKnight : ChessPiece.BKnight;
            ChessPiece mybishop = board.WhosTurn == ChessPlayer.White ? ChessPiece.WBishop : ChessPiece.BBishop;
            ChessPiece myrook = board.WhosTurn == ChessPlayer.White ? ChessPiece.WRook : ChessPiece.BRook;
            ChessPiece myqueen = board.WhosTurn == ChessPlayer.White ? ChessPiece.WQueen : ChessPiece.BQueen;
            ChessPiece myking = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKing : ChessPiece.BKing;

            ChessDirection mynorth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirN : ChessDirection.DirS;
            ChessDirection mysouth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirS : ChessDirection.DirN;
            ChessRank myrank4 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank4 : ChessRank.Rank5;


            ChessPosition tmppos;
            ChessPiece tmppiece;
            ChessFile tmpfile;
            ChessRank tmprank;

            if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678]$", RegexOptions.IgnoreCase))
            {
                //coordinate notation, will not verify legality for now
                From = ChessPositionInfo.Parse(movetext.Substring(0, 2));
                To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
            }
            else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678][BNRQK]$", RegexOptions.IgnoreCase))
            {
                //coordinate notation, with promotion
                From = ChessPositionInfo.Parse(movetext.Substring(0, 2));
                To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
                Promote = movetext[4].ParseAsPiece(me);
            }
            else if (movetext == "0-0" || movetext == "O-O" || movetext == "o-o")
            {
                if (me == ChessPlayer.White)
                {
                    From = ChessPosition.E1;
                    To = ChessPosition.G1;
                }
                else
                {
                    From = ChessPosition.E8;
                    To = ChessPosition.G8;
                }
            }
            else if (movetext == "0-0-0" || movetext == "O-O-O" || movetext == "o-o-o")
            {
                if (me == ChessPlayer.White)
                {
                    From = ChessPosition.E1;
                    To = ChessPosition.C1;
                }
                else
                {
                    From = ChessPosition.E8;
                    To = ChessPosition.C8;
                }
            }
            else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678]$"))
            {
                //pawn forward
                To = ChessPositionInfo.Parse(movetext);
                tmppos = To.PositionInDirection(mysouth);
                if (board.PieceAt(tmppos) == mypawn)
                {
                    From = tmppos;
                    return Create(From, To);
                }
                else if (board.PieceAt(tmppos) == ChessPiece.EMPTY && To.GetRank() == myrank4)
                {
                    tmppos = tmppos.PositionInDirection(mysouth);
                    if (board.PieceAt(tmppos) == mypawn)
                    {
                        From = tmppos;
                        return Create(From, To);
                    }
                }
                throw new ArgumentException("no pawn can move to " + movetext);

            }
            else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][BNRQK]$"))
            {
                //pawn forward, promotion
                To = ChessPositionInfo.Parse(movetext.Substring(0, 2));
                tmppos = To.PositionInDirection(mysouth);
                if (board.PieceAt(tmppos) == mypawn)
                {
                    From = tmppos;
                    Promote = movetext[2].ParseAsPiece(me);
                    return Create(From, To, Promote);
                }
                throw new ArgumentException("no pawn can promoted to " + movetext.Substring(0, 2));
            }
            else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678]$"))
            {
                //pawn attack
                To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
                tmpfile = ChessFileInfo.Parse(movetext[0]);
                From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
                return Create(From, To);
            }
            else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678][BNRQK]$"))
            {
                //pawn attack, promote
                To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
                tmpfile = ChessFileInfo.Parse(movetext[0]);
                From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
                Promote = movetext[3].ParseAsPiece(me);
                return Create(From, To);
            }
            else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678]$"))
            {
                //normal attack
                To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
                tmppiece = movetext[0].ParseAsPiece(me);
                From = filter(board, To, tmppiece, ChessFile.EMPTY, ChessRank.EMPTY);
                return Create(From, To);
            }
            else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][abcdefgh][12345678]$"))
            {
                //normal, specify file
                To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
                tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = ChessFileInfo.Parse(movetext[1]);
                From = filter(board, To, tmppiece, tmpfile, ChessRank.EMPTY);
                return Create(From, To);
            }
            else if (Regex.IsMatch(movetext, "^[BNRQK][12345678][abcdefgh][12345678]$"))
            {
                //normal, specify rank
                To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
                tmppiece = movetext[0].ParseAsPiece(me);
                tmprank = ChessRankInfo.Parse(movetext[1]);
                From = filter(board, To, tmppiece, ChessFile.EMPTY, tmprank);
                return Create(From, To);

            }
            else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678][abcdefgh][12345678]$"))
            {
                //normal, specify rank and file
                To = ChessPositionInfo.Parse(movetext.Substring(3, 2));
                tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = ChessFileInfo.Parse(movetext[1]);
                tmprank = ChessRankInfo.Parse(movetext[2]);
                From = filter(board, To, tmppiece, tmpfile, tmprank);
                return Create(From, To);
            }
            return Create(From, To);
            //throw new ArgumentException(movetext + " not a valid move string");

        }

        private static ChessPosition filter(ChessBoard board, ChessPosition attackto, ChessPiece piece, ChessFile file, ChessRank rank)
        {
            List<ChessPosition> fits = new List<ChessPosition>();
            var attacksTo = board.AttacksTo(attackto, board.WhosTurn);
            foreach (ChessPosition pos in attacksTo.ToPositions())
            {
                if (piece != ChessPiece.EMPTY && piece != board.PieceAt(pos))
                {
                    continue;
                }
                if (rank != ChessRank.EMPTY && rank != pos.GetRank())
                {
                    continue;
                }
                if (file != ChessFile.EMPTY && file != pos.GetFile())
                {
                    continue;
                }
                fits.Add(pos);
            }

            if (fits.Count > 1)
            {
                //ambigous moves, one is probably illegal, check against legal move list
                ChessMoves allLegal = new ChessMoves(ChessMoveInfo.GenMovesLegal(board));
                fits.Clear();
                foreach (ChessMove move in allLegal)
                {
                    if (move.To() != attackto) { continue; }
                    if (board.PieceAt(move.From()) != piece) { continue; }
                    if (file != ChessFile.EMPTY && move.From().GetFile() != file) { continue; }
                    if (rank != ChessRank.EMPTY && move.From().GetRank() != rank) { continue; }
                    fits.Add(move.From());
                }
            }

            if (fits.Count != 1)
            {
                throw new ArgumentException("invalid move input");

            }

            return fits[0];
        }

        public static string Write(this ChessMove move)
        {
            string retval = "";
            if (move.Promote() == ChessPiece.EMPTY)
            {
                retval = move.From().PositionToString().ToLower() + move.To().PositionToString().ToLower();
            }
            else
            {
                retval = move.From().PositionToString().ToLower() + move.To().PositionToString().ToLower() + move.Promote().PieceToString().ToLower();
            }
            return retval;
        }

    }
}
