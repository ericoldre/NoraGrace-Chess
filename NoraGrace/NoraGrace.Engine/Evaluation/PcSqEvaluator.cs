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

        public void PcSqValuesAdd(Piece piece, Position pos, ref PhasedScore value)
        {
            value = value.Add(_pcsqPiecePos[(int)piece][(int)pos]);
        }
        public void PcSqValuesRemove(Piece piece, Position pos, ref PhasedScore value)
        {
            value = value.Subtract(_pcsqPiecePos[(int)piece][(int)pos]);
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
