using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;

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
                retval[i] = this[i].fnGetValue(settings);
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
                var initial = this[i].fnGetValue(settings);
                this[i].fnSetValue(settings, values[i]);
                var after = this[i].fnGetValue(settings);
                var x = after * 2;
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
        public Func<Engine.Evaluation.Settings, double> fnGetValue { get; protected set; }
        public Action<Engine.Evaluation.Settings, double> fnSetValue { get; protected set; }
        public double Increment { get; protected set; }
        public string Name { get; protected set; }



        public static readonly TunableParameter KingAttackCountValue = new TunableParameter()
        {
            Name = "KingAttackCountValue",
            fnGetValue = (s) => s.KingAttackCountValue,
            fnSetValue = (s, v) => { s.KingAttackCountValue = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingAttackFactor = new TunableParameter()
        {
            Name = "KingAttackFactor",
            fnGetValue = (s) => s.KingAttackFactor,
            fnSetValue = (s, v) => { s.KingAttackFactor = v; },
            Increment = .15
        };

        public static readonly TunableParameter KingAttackFactorQueenTropismBonus = new TunableParameter()
        {
            Name = "KingAttackFactorQueenTropismBonus",
            fnGetValue = (s) => s.KingAttackFactorQueenTropismBonus,
            fnSetValue = (s, v) => { s.KingAttackFactorQueenTropismBonus = v; },
            Increment = .15
        };

        public static readonly TunableParameter KingAttackWeightCutoff = new TunableParameter()
        {
            Name = "KingAttackWeightCutoff",
            fnGetValue = (s) => s.KingAttackWeightCutoff,
            fnSetValue = (s, v) => { s.KingAttackWeightCutoff = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingAttackWeightValue = new TunableParameter()
        {
            Name = "KingAttackWeightValue",
            fnGetValue = (s) => s.KingAttackWeightValue,
            fnSetValue = (s, v) => { s.KingAttackWeightValue = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingRingAttack = new TunableParameter()
        {
            Name = "KingRingAttack",
            fnGetValue = (s) => s.KingRingAttack,
            fnSetValue = (s, v) => { s.KingRingAttack = (int)v; },
            Increment = 1
        };

        public static readonly TunableParameter KingRingAttackControlBonus = new TunableParameter()
        {
            Name = "KingRingAttackControlBonus",
            fnGetValue = (s) => s.KingRingAttackControlBonus,
            fnSetValue = (s, v) => { s.KingRingAttackControlBonus = (int)v; },
            Increment = 1
        };



    }

    public class TunableParameterMaterial : TunableParameter
    {
        public GameStage GameStage { get; private set; }
        public PieceType PieceType { get; private set; }

        public TunableParameterMaterial(GameStage gameStage, PieceType pieceType)
        {
            GameStage = gameStage;
            PieceType = pieceType;

            this.Name = string.Format("Material[PieceType={0},Position={1}", GameStage, PieceType);
            this.Increment = 5;
            this.fnGetValue = (s) => s.MaterialValues[PieceType][GameStage];
            this.fnSetValue = (s, v) => { s.MaterialValues[PieceType][GameStage] = (int)v; };
        }
    }

    public class TunableParameterPcSq: TunableParameter
    {
        public GameStage GameStage { get; private set; }
        public PieceType PieceType { get; private set; }
        public Position Position { get; private set; }

        public TunableParameterPcSq(GameStage gameStage, PieceType pieceType, Position position)
        {
            GameStage = gameStage;
            PieceType = pieceType;
            Position = position;

            this.Name = string.Format("PcSq[GameStage={0},PieceType={1},Position={2}", GameStage, PieceType, Position);
            this.Increment = 3;
            this.fnGetValue = (s) => s.PcSqTables[PieceType][GameStage][Position];
            this.fnSetValue = (s, v) => { s.PcSqTables[PieceType][GameStage][Position] = (int)v; };
        }
    }
}
