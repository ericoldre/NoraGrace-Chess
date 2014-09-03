using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation
{
    public class PcSqEvaluator
    {
        private readonly PhasedScore[][] _pcsqPiecePos = new PhasedScore[PieceUtil.LookupArrayLength][];
        private static readonly int[] _endgameMateKingPcSq;


        public void PcSqValuesAdd(Piece piece, Position pos, ref PhasedScore value)
        {
            value = value.Add(_pcsqPiecePos[(int)piece][(int)pos]);
        }
        public void PcSqValuesRemove(Piece piece, Position pos, ref PhasedScore value)
        {
            value = value.Subtract(_pcsqPiecePos[(int)piece][(int)pos]);
        }

        public static PhasedScore EndgameMateKingPcSq(Position loseKing, Position winKing)
        {
            return PhasedScoreUtil.Create(0, _endgameMateKingPcSq[(int)loseKing] - (winKing.DistanceTo(loseKing) * 25));
        }

        public static bool UseEndGamePcSq(Board board, Player winPlayer, out PhasedScore newPcSq)
        {
            Player losePlayer = winPlayer.PlayerOther();
            if (
                board.PieceCount(losePlayer, PieceType.Pawn) == 0
                && board.PieceCount(losePlayer, PieceType.Queen) == 0
                && board.PieceCount(losePlayer, PieceType.Rook) == 0
                && (board.PieceCount(losePlayer, PieceType.Bishop) + board.PieceCount(losePlayer, PieceType.Knight) <= 1))
            {
                if (board.PieceCount(winPlayer, PieceType.Queen) > 0
                    || board.PieceCount(winPlayer, PieceType.Rook) > 0
                    || board.PieceCount(winPlayer, PieceType.Bishop) + board.PieceCount(winPlayer, PieceType.Bishop) >= 2)
                {
                    Position loseKing = board.KingPosition(losePlayer);
                    Position winKing = board.KingPosition(winPlayer);
                    newPcSq = PcSqEvaluator.EndgameMateKingPcSq(loseKing, winKing);
                    return true;
                }
            }
            newPcSq = 0;
            return false;
        }

        static PcSqEvaluator()
        {
            //initialize pcsq for trying to mate king in endgame, try to push it to edge of board.
            _endgameMateKingPcSq = new int[64];
            foreach (var pos in PositionUtil.AllPositions)
            {
                List<int> distToMid = new List<int>();
                distToMid.Add(pos.DistanceToNoDiag(Position.D4));
                distToMid.Add(pos.DistanceToNoDiag(Position.D5));
                distToMid.Add(pos.DistanceToNoDiag(Position.E4));
                distToMid.Add(pos.DistanceToNoDiag(Position.E5));
                var minDist = distToMid.Min();
                _endgameMateKingPcSq[(int)pos] = minDist * 50;
            }
        }

        public PcSqEvaluator(Settings settings)
        {
            Action<Settings.PcSqDictionary> actionNormalizePcSq = (data) =>
            {
                int sum = PositionUtil.AllPositions.Sum(p => data[p]);
                int per = sum / 64;
                data.Offset -= per;
            };
            foreach (PieceType pieceType in PieceTypeUtil.AllPieceTypes)
            {
                actionNormalizePcSq(settings.PcSqTables[pieceType][GameStage.Opening]);
                actionNormalizePcSq(settings.PcSqTables[pieceType][GameStage.Endgame]);
            }

            //setup piecesq tables
            foreach (Piece piece in PieceUtil.AllPieces)
            {
                _pcsqPiecePos[(int)piece] = new PhasedScore[64];
                foreach (Position pos in PositionUtil.AllPositions)
                {

                    if (piece.PieceToPlayer() == Player.White)
                    {
                        _pcsqPiecePos[(int)piece][(int)pos] = PhasedScoreUtil.Create(
                            settings.PcSqTables[piece.ToPieceType()][GameStage.Opening][pos],
                            settings.PcSqTables[piece.ToPieceType()][GameStage.Endgame][pos]);
                    }
                    else
                    {
                        _pcsqPiecePos[(int)piece][(int)pos] = PhasedScoreUtil.Create(
                            -settings.PcSqTables[piece.ToPieceType()][GameStage.Opening][pos.Reverse()],
                            -settings.PcSqTables[piece.ToPieceType()][GameStage.Endgame][pos.Reverse()]);
                    }
                }
            }
        }
    }
}
