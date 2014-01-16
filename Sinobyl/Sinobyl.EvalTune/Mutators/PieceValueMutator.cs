using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

namespace Sinobyl.EvalTune.Mutators
{

    public class PieceValueMutator: IEvalSettingsMutator
    {
        public ChessPieceType PieceType { get; private set; }
        public ChessGameStage[] Stages { get; private set; }
        public int Amount { get; private set; }

        public PieceValueMutator(ChessPieceType pieceType, IEnumerable<ChessGameStage> stages, int amount)
        {
            this.PieceType = pieceType;
            this.Stages = stages.ToArray();
            this.Amount = amount;
        }

        public PieceValueMutator(Random rand)
        {
            PieceType = ChessPieceInfo.AllPieces[rand.Next(0, ChessPieceInfo.AllPieces.Count())].ToPieceType();

            switch (rand.Next(0, 3))
            {
                case 0:
                    Stages = new ChessGameStage[] { ChessGameStage.Opening };
                    break;
                case 1:
                    Stages = new ChessGameStage[] { ChessGameStage.Endgame };
                    break;
                default:
                    Stages = new ChessGameStage[] { ChessGameStage.Opening, ChessGameStage.Endgame };
                    break;
            }

            while(Math.Abs(Amount)<5)
            {
                Amount = rand.Next(-20, 21);
            }
            
        }

        #region IEvalSettingsMutator Members

        void IEvalSettingsMutator.Mutate(Engine.ChessEvalSettings settings)
        {
            foreach (ChessGameStage stage in this.Stages)
            {
                settings.MaterialValues[this.PieceType][stage] += Amount;
            }
        }

        IEnumerable<IEvalSettingsMutator> IEvalSettingsMutator.SimilarMutators()
        {
            yield return new PieceValueMutator(this.PieceType, new ChessGameStage[] { ChessGameStage.Opening }, this.Amount);
            yield return new PieceValueMutator(this.PieceType, new ChessGameStage[] { ChessGameStage.Endgame }, this.Amount);
            yield return new PieceValueMutator(this.PieceType, new ChessGameStage[] { ChessGameStage.Opening, ChessGameStage.Endgame }, this.Amount);

        }
        #endregion

        public override string ToString()
        {
            return string.Format("Material[{0}][{1}] {2}", this.PieceType.ToString(), this.Stages.Count() > 1 ? "BOTH" : this.Stages[0].ToString(), this.Amount > 0 ? "+= " + this.Amount.ToString() : "-= " + Math.Abs(this.Amount).ToString());
        }
    }
}
