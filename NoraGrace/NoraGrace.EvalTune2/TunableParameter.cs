using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.EvalTune2
{

    public class TunableParameterList : List<TunableParameter>
    {

        public double[] CreateDefaultValues()
        {
            double[] retval = new double[this.Count];
            Engine.Evaluation.Settings settings = Engine.Evaluation.Settings.Default();
            for (int i = 0; i < this.Count; i++)
            {
                retval[i] = this[i].fnGetDefault(settings);
            }
            return retval;
        }

        public double[] CreateIncrements()
        {
            double[] retval = new double[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                retval[i] = this[i].Increment;
            }
            return retval;
        }

        public Engine.Evaluation.Evaluator CreateEvaluator(double[] values)
        {
            var settings = CreateSettings(values);
            return new Engine.Evaluation.Evaluator(settings);
        }

        public Engine.Evaluation.Settings CreateSettings(double[] values)
        {
            Engine.Evaluation.Settings settings = Engine.Evaluation.Settings.Default();

            for (int i = 0; i < this.Count; i++)
            {
                this[i].fnSetValue(settings, values[i]);
            }
            return settings;
        }

        public string FormatValues(double[] values)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                sb.AppendFormat("{0,5} {1}", values[i], this[i].Name);
            }
            return sb.ToString();
        }

        public void WriteToFile(string fileName, double[] values, double e, int iteration)
        {
            using (var writer = new System.IO.StreamWriter(fileName, true))
            {
                writer.WriteLine();
                writer.WriteLine(DateTime.Now.ToString());
                writer.WriteLine("E={0}", e);
                writer.WriteLine("iteration={0}", iteration);
                for (int i = 0; i < this.Count; i++)
                {
                    writer.WriteLine("{0,5} {1}", values[i], this[i].Name);
                }
            }
        }
    }

    public class TunableParameter
    {
        public Func<Engine.Evaluation.Settings, double> fnGetDefault { get; private set; }
        public Action<Engine.Evaluation.Settings, double> fnSetValue { get; private set; }
        public double Increment { get; private set; }
        public string Name { get; private set; }



        public static readonly TunableParameter KingAttackCountValue = new TunableParameter()
        {
            Name = "KingAttackCountValue",
            fnGetDefault = (s) => s.KingAttackCountValue,
            fnSetValue = (s, v) => { s.KingAttackCountValue = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingAttackFactor = new TunableParameter()
        {
            Name = "KingAttackFactor",
            fnGetDefault = (s) => s.KingAttackFactor,
            fnSetValue = (s, v) => { s.KingAttackFactor = (int)v; },
            Increment = .15
        };

        public static readonly TunableParameter KingAttackFactorQueenTropismBonus = new TunableParameter()
        {
            Name = "KingAttackFactorQueenTropismBonus",
            fnGetDefault = (s) => s.KingAttackFactorQueenTropismBonus,
            fnSetValue = (s, v) => { s.KingAttackFactorQueenTropismBonus = (int)v; },
            Increment = .15
        };

        public static readonly TunableParameter KingAttackWeightCutoff = new TunableParameter()
        {
            Name = "KingAttackWeightCutoff",
            fnGetDefault = (s) => s.KingAttackWeightCutoff,
            fnSetValue = (s, v) => { s.KingAttackWeightCutoff = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingAttackWeightValue = new TunableParameter()
        {
            Name = "KingAttackWeightValue",
            fnGetDefault = (s) => s.KingAttackWeightValue,
            fnSetValue = (s, v) => { s.KingAttackWeightValue = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingRingAttack = new TunableParameter()
        {
            Name = "KingRingAttack",
            fnGetDefault = (s) => s.KingRingAttack,
            fnSetValue = (s, v) => { s.KingRingAttack = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingRingAttackControlBonus = new TunableParameter()
        {
            Name = "KingRingAttackControlBonus",
            fnGetDefault = (s) => s.KingRingAttackControlBonus,
            fnSetValue = (s, v) => { s.KingRingAttackControlBonus = (int)v; },
            Increment = 1
        };


    }
}
