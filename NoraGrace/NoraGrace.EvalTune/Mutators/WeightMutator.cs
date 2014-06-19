using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoraGrace.Engine;

namespace NoraGrace.EvalTune.Mutators
{
    class WeightMutator : IEvalSettingsMutator
    {
        public string WeightType { get; set; }
        public int Amount { get; set; }
        public GameStage Stage { get; set; }

        public WeightMutator()
        {
            WeightType = "Material";
            Stage = GameStage.Opening;
            Amount = 10;
        }
        public WeightMutator(Random rand)
        {
            
            switch (rand.Next(0, 3))
            {
                case 0:
                    WeightType = "Material";
                    break;
                case 1:
                    WeightType = "PcSq";
                    break;
                default:
                    WeightType = "Mobility";
                    break;
            }

            switch (rand.Next(0,2))
            {
                case 0:
                    Stage = GameStage.Opening;
                    break;
                case 1:
                    Stage = GameStage.Endgame;
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
            switch (this.WeightType)
            {
                case "Material":
                    settings.Weight.Material[this.Stage] += this.Amount;
                    break;
                case "PcSq":
                    settings.Weight.PcSq[this.Stage] += this.Amount;
                    break;
                case "Mobility":
                    settings.Weight.Mobility[this.Stage] += this.Amount;
                    break;
                default:
                    throw new Exception("invalid weight type");
            }
        }

        IEnumerable<IEvalSettingsMutator> IEvalSettingsMutator.SimilarMutators()
        {
            yield return this;
            yield return new WeightMutator() { WeightType = this.WeightType, Amount = this.Amount, Stage = this.Stage.Other() };
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Weight[{0}][{1}] {2}", WeightType, this.Stage.ToString(), this.Amount > 0 ? "+= " + this.Amount.ToString() : "-= " + Math.Abs(this.Amount).ToString());
        }
    }
}
