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

        public string[] CreateNames()
        {
            string[] retval = new string[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                retval[i] = this[i].Name;
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


        public static IEnumerable<TunableParameter> KingSafetyParams()
        {
            yield return new TunableParameter()
            {
                Name = "KingAttackCountValue",
                fnGetValue = (s) => s.KingAttackCountValue,
                fnSetValue = (s, v) => { s.KingAttackCountValue = (int)v; },
                Increment = 1
            };
            yield return new TunableParameter()
            {
                Name = "KingAttackFactor",
                fnGetValue = (s) => s.KingAttackFactor,
                fnSetValue = (s, v) => { s.KingAttackFactor = v; },
                Increment = .15
            };
            yield return new TunableParameter()
            {
                Name = "KingAttackFactorQueenTropismBonus",
                fnGetValue = (s) => s.KingAttackFactorQueenTropismBonus,
                fnSetValue = (s, v) => { s.KingAttackFactorQueenTropismBonus = v; },
                Increment = .15
            };
            yield return new TunableParameter()
            {
                Name = "KingAttackWeightCutoff",
                fnGetValue = (s) => s.KingAttackWeightCutoff,
                fnSetValue = (s, v) => { s.KingAttackWeightCutoff = (int)v; },
                Increment = 1
            };
            yield return new TunableParameter()
            {
                Name = "KingAttackWeightValue",
                fnGetValue = (s) => s.KingAttackWeightValue,
                fnSetValue = (s, v) => { s.KingAttackWeightValue = (int)v; },
                Increment = 1
            };
            yield return new TunableParameter()
            {
                Name = "KingRingAttack",
                fnGetValue = (s) => s.KingRingAttack,
                fnSetValue = (s, v) => { s.KingRingAttack = (int)v; },
                Increment = 1
            };
            yield return new TunableParameter()
            {
                Name = "KingRingAttackControlBonus",
                fnGetValue = (s) => s.KingRingAttackControlBonus,
                fnSetValue = (s, v) => { s.KingRingAttackControlBonus = (int)v; },
                Increment = 1
            };
        }

        public static IEnumerable<TunableParameter> MaterialParams()
        {
            yield return new TunableParameter()
            {
                Name = "BishopPairOpening",
                fnGetValue = (s) => s.MaterialBishopPair.Opening,
                fnSetValue = (s, v) => { s.MaterialBishopPair.Opening = (int)v; },
                Increment = 5
            };
            yield return new TunableParameter()
            {
                Name = "BishopPairEndgame",
                fnGetValue = (s) => s.MaterialBishopPair.Endgame,
                fnSetValue = (s, v) => { s.MaterialBishopPair.Endgame = (int)v; },
                Increment = 5
            };
            //foreach (var pt in PieceTypeUtil.AllPieceTypes.Where(t => t != PieceType.King))
            //{
            //     yield return new TunableParameterMaterial(GameStage.Opening, pt);
            //     yield return new TunableParameterMaterial(GameStage.Endgame, pt);
            //}
        }

        public static IEnumerable<TunableParameter> MobilityParams()
        {
            foreach (var pt in PieceTypeUtil.AllPieceTypes.Where(o=>o != PieceType.Pawn && o != PieceType.King))
            {
                foreach (var gs in GameStageUtil.AllGameStages)
                {
                    yield return new TunableParameter()
                    {
                        Name = string.Format("mob{0}{1}Amp",pt,gs),
                        fnGetValue = (s) => s.Mobility[pt][gs].Amplitude,
                        fnSetValue = (s, v) => { s.Mobility[pt][gs].Amplitude = (int)v; },
                        Increment = 4
                    };

                    yield return new TunableParameter()
                    {
                        Name = string.Format("mob{0}{1}X", pt, gs),
                        fnGetValue = (s) => s.Mobility[pt][gs].BezControlPct.X,
                        fnSetValue = (s, v) => { s.Mobility[pt][gs].BezControlPct.X = v; },
                        Increment = .1
                    };

                    yield return new TunableParameter()
                    {
                        Name = string.Format("mob{0}{1}Y", pt, gs),
                        fnGetValue = (s) => s.Mobility[pt][gs].BezControlPct.Y,
                        fnSetValue = (s, v) => { s.Mobility[pt][gs].BezControlPct.Y = v; },
                        Increment = .1
                    };
                }
            }

        }

        public static IEnumerable<TunableParameter> PawnParams()
        {
            yield return new TunableParameter()
            {
                Name = "PawnDoubledOpening",
                fnGetValue = (s) => s.PawnDoubled.Opening,
                fnSetValue = (s, v) => { s.PawnDoubled.Opening = (int)v; },
                Increment = 2
            };
            yield return new TunableParameter()
            {
                Name = "PawnDoubledEndgame",
                fnGetValue = (s) => s.PawnDoubled.Endgame,
                fnSetValue = (s, v) => { s.PawnDoubled.Endgame = (int)v; },
                Increment = 2
            };

            yield return new TunableParameter()
            {
                Name = "PawnIsolatedOpening",
                fnGetValue = (s) => s.PawnIsolated.Opening,
                fnSetValue = (s, v) => { s.PawnIsolated.Opening = (int)v; },
                Increment = 2
            };
            yield return new TunableParameter()
            {
                Name = "PawnIsolatedEndgame",
                fnGetValue = (s) => s.PawnIsolated.Endgame,
                fnSetValue = (s, v) => { s.PawnIsolated.Endgame = (int)v; },
                Increment = 2
            };

            yield return new TunableParameter()
            {
                Name = "PawnUnconnectedOpening",
                fnGetValue = (s) => s.PawnUnconnected.Opening,
                fnSetValue = (s, v) => { s.PawnUnconnected.Opening = (int)v; },
                Increment = 2
            };
            yield return new TunableParameter()
            {
                Name = "PawnUnconnectedEndgame",
                fnGetValue = (s) => s.PawnUnconnected.Endgame,
                fnSetValue = (s, v) => { s.PawnUnconnected.Endgame = (int)v; },
                Increment = 2
            };

            yield return new TunableParameter()
            {
                Name = "PawnShelterFactor",
                fnGetValue = (s) => s.PawnShelterFactor,
                fnSetValue = (s, v) => { s.PawnShelterFactor = (int)v; },
                Increment = 1
            };


        }

        public static IEnumerable<TunableParameter> PassedPawnParams()
        {
            yield return new TunableParameter()
            {
                Name = "PawnPassed8thRankScore",
                fnGetValue = (s) => s.PawnPassed8thRankScore,
                fnSetValue = (s, v) => { s.PawnPassed8thRankScore = (int)v; },
                Increment = 5
            };

            yield return new TunableParameter()
            {
                Name = "PawnPassedClosePct",
                fnGetValue = (s) => s.PawnPassedClosePct,
                fnSetValue = (s, v) => { s.PawnPassedClosePct = v; },
                Increment = .05
            };

            yield return new TunableParameter()
            {
                Name = "PawnPassedFarPct",
                fnGetValue = (s) => s.PawnPassedFarPct,
                fnSetValue = (s, v) => { s.PawnPassedFarPct = v; },
                Increment = .05
            };

            yield return new TunableParameter()
            {
                Name = "PawnPassedDangerPct",
                fnGetValue = (s) => s.PawnPassedDangerPct,
                fnSetValue = (s, v) => { s.PawnPassedDangerPct = v; },
                Increment = .01
            };

            

            yield return new TunableParameter()
            {
                Name = "PawnPassedMinScore",
                fnGetValue = (s) => s.PawnPassedMinScore,
                fnSetValue = (s, v) => { s.PawnPassedMinScore = (int)v; },
                Increment = 2
            };

            yield return new TunableParameter()
            {
                Name = "PawnPassedOpeningPct",
                fnGetValue = (s) => s.PawnPassedOpeningPct,
                fnSetValue = (s, v) => { s.PawnPassedOpeningPct = v; },
                Increment = .03
            };

            yield return new TunableParameter()
            {
                Name = "PawnPassedRankReduction",
                fnGetValue = (s) => s.PawnPassedRankReduction,
                fnSetValue = (s, v) => { s.PawnPassedRankReduction = v; },
                Increment = .03
            };

            yield return new TunableParameter()
            {
                Name = "PawnCandidatePct",
                fnGetValue = (s) => s.PawnCandidatePct,
                fnSetValue = (s, v) => { s.PawnCandidatePct = v; },
                Increment = .05
            };

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
