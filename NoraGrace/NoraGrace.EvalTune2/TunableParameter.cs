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

        public enum PcSqTuneType
        {
            r1, r2, r3, r4, r5, r6, r7, r8,
            fah, fbg, fcf, fed,
            c4, cb, oe
        }

        public GameStage GameStage { get; private set; }
        public PieceType PieceType { get; private set; }
        public PcSqTuneType TuneType { get; private set; }

        public TunableParameterPcSq(GameStage gameStage, PieceType pieceType, PcSqTuneType tuneType)
        {
            GameStage = gameStage;
            PieceType = pieceType;
            TuneType = tuneType;

            this.Name = string.Format("PcSq[GameStage={0},PieceType={1},TuneType={2}", GameStage, PieceType, TuneType);
            this.Increment = 3;
            this.fnGetValue = (s) =>
            {
                switch (TuneType)
                {
                    case PcSqTuneType.r1:
                        return s.PcSqTables[PieceType][GameStage].Rank1;
                    case PcSqTuneType.r2:
                        return s.PcSqTables[PieceType][GameStage].Rank2;
                    case PcSqTuneType.r3:
                        return s.PcSqTables[PieceType][GameStage].Rank3;
                    case PcSqTuneType.r4:
                        return s.PcSqTables[PieceType][GameStage].Rank4;
                    case PcSqTuneType.r5:
                        return s.PcSqTables[PieceType][GameStage].Rank5;
                    case PcSqTuneType.r6:
                        return s.PcSqTables[PieceType][GameStage].Rank6;
                    case PcSqTuneType.r7:
                        return s.PcSqTables[PieceType][GameStage].Rank7;
                    case PcSqTuneType.r8:
                        return s.PcSqTables[PieceType][GameStage].Rank8;
                    case PcSqTuneType.fah:
                        return s.PcSqTables[PieceType][GameStage].FileAH;
                    case PcSqTuneType.fbg:
                        return s.PcSqTables[PieceType][GameStage].FileBG;
                    case PcSqTuneType.fcf:
                        return s.PcSqTables[PieceType][GameStage].FileCF;
                    case PcSqTuneType.fed:
                        return s.PcSqTables[PieceType][GameStage].FileDE;
                    case PcSqTuneType.c4:
                        return s.PcSqTables[PieceType][GameStage].Center4;
                    case PcSqTuneType.cb:
                        return s.PcSqTables[PieceType][GameStage].CenterBorder;
                    case PcSqTuneType.oe:
                        return s.PcSqTables[PieceType][GameStage].OutsideEdge;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
            this.fnSetValue = (s, v) => 
            {
                switch (TuneType)
                {
                    case PcSqTuneType.r1:
                        s.PcSqTables[PieceType][GameStage].Rank1 = (int)v;
                        break;
                    case PcSqTuneType.r2:
                        s.PcSqTables[PieceType][GameStage].Rank2 = (int)v;
                        break;
                    case PcSqTuneType.r3:
                        s.PcSqTables[PieceType][GameStage].Rank3 = (int)v;
                        break;
                    case PcSqTuneType.r4:
                        s.PcSqTables[PieceType][GameStage].Rank4 = (int)v;
                        break;
                    case PcSqTuneType.r5:
                        s.PcSqTables[PieceType][GameStage].Rank5 = (int)v;
                        break;
                    case PcSqTuneType.r6:
                        s.PcSqTables[PieceType][GameStage].Rank6 = (int)v;
                        break;
                    case PcSqTuneType.r7:
                        s.PcSqTables[PieceType][GameStage].Rank7 = (int)v;
                        break;
                    case PcSqTuneType.r8:
                        s.PcSqTables[PieceType][GameStage].Rank8 = (int)v;
                        break;
                    case PcSqTuneType.fah:
                        s.PcSqTables[PieceType][GameStage].FileAH = (int)v;
                        break;
                    case PcSqTuneType.fbg:
                        s.PcSqTables[PieceType][GameStage].FileBG = (int)v;
                        break;
                    case PcSqTuneType.fcf:
                        s.PcSqTables[PieceType][GameStage].FileCF = (int)v;
                        break;
                    case PcSqTuneType.fed:
                        s.PcSqTables[PieceType][GameStage].FileDE = (int)v;
                        break;
                    case PcSqTuneType.c4:
                        s.PcSqTables[PieceType][GameStage].Center4 = (int)v;
                        break;
                    case PcSqTuneType.cb:
                        s.PcSqTables[PieceType][GameStage].CenterBorder = (int)v;
                        break;
                    case PcSqTuneType.oe:
                        s.PcSqTables[PieceType][GameStage].OutsideEdge = (int)v;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        public static IEnumerable<TunableParameterPcSq> SelectAll()
        {
            foreach (var gs in GameStageUtil.AllGameStages)
            {
                foreach (var pt in PieceTypeUtil.AllPieceTypes)
                {
                    if (pt != PieceType.Pawn) { yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r1); }
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r2);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r3);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r4);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r5);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r6);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r7);
                    if (pt != PieceType.Pawn) { yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.r8); }
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.fah);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.fbg);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.fcf);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.fed);

                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.c4);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.cb);
                    yield return new TunableParameterPcSq(gs, pt, PcSqTuneType.oe);
                }
            }
        }
    }
}
