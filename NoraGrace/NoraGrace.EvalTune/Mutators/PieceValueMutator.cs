using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;
namespace NoraGrace.EvalTune.Mutators
{

    public class PieceValueMutator: IEvalSettingsMutator
    {
        public PieceType PieceType { get; private set; }
        public NoraGrace.Engine.Evaluation.GameStage[] Stages { get; private set; }
        public int Amount { get; private set; }

        public PieceValueMutator(PieceType pieceType, IEnumerable<GameStage> stages, int amount)
        {
            this.PieceType = pieceType;
            this.Stages = stages.ToArray();
            this.Amount = amount;
        }

        public PieceValueMutator(Random rand)
        {
            PieceType = PieceInfo.AllPieces[rand.Next(0, PieceInfo.AllPieces.Count())].ToPieceType();

            switch (rand.Next(0, 3))
            {
                case 0:
                    Stages = new GameStage[] { GameStage.Opening };
                    break;
                case 1:
                    Stages = new GameStage[] { GameStage.Endgame };
                    break;
                default:
                    Stages = new GameStage[] { GameStage.Opening, GameStage.Endgame };
                    break;
            }

            while(Math.Abs(Amount)<5)
            {
                Amount = rand.Next(-20, 21);
            }
            
        }

        #region IEvalSettingsMutator Members

        void IEvalSettingsMutator.Mutate(Engine.Evaluation.Settings settings)
        {
            foreach (GameStage stage in this.Stages)
            {
                settings.MaterialValues[this.PieceType][stage] += Amount;
            }
        }

        IEnumerable<IEvalSettingsMutator> IEvalSettingsMutator.SimilarMutators()
        {
            yield return new PieceValueMutator(this.PieceType, new GameStage[] { GameStage.Opening }, this.Amount);
            yield return new PieceValueMutator(this.PieceType, new GameStage[] { GameStage.Endgame }, this.Amount);
            yield return new PieceValueMutator(this.PieceType, new GameStage[] { GameStage.Opening, GameStage.Endgame }, this.Amount);

        }
        #endregion

        public override string ToString()
        {
            return string.Format("Material[{0}][{1}] {2}", this.PieceType.ToString(), this.Stages.Count() > 1 ? "BOTH" : this.Stages[0].ToString(), this.Amount > 0 ? "+= " + this.Amount.ToString() : "-= " + Math.Abs(this.Amount).ToString());
        }
    }
}
