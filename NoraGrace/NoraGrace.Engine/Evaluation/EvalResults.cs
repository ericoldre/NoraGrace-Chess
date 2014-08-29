using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation
{
    public class EvalResults
    {

        public int[] Workspace = new int[64];


        //basic eval terms:
        public long Zobrist { get; private set; }
        public int DrawScore { get; private set; }
        public int Material { get; private set; }
        public PhasedScore PcSq { get; set; }
        public PhasedScore Pawns { get; private set; }
        public ScaleFactor StageStartWeight { get; private set; }
        public ScaleFactor ScaleWhite { get; private set; }
        public ScaleFactor ScaleBlack { get; private set; }
        public Bitboard PassedPawns { get; private set; }   //not a score but information
        public Bitboard CandidatePawns { get; private set; }  //not a score but information

        //advanced eval terms.
        public int LazyAge { get; set; }
        public ChessEvalAttackInfo[] Attacks = new ChessEvalAttackInfo[] { new ChessEvalAttackInfo(), new ChessEvalAttackInfo() };
        public PhasedScore PawnsPassed = 0;
        public PhasedScore ShelterStorm = 0;

        public void Reset()
        {
            Attacks[0].Reset();
            Attacks[1].Reset();
            Material = 0;
            PcSq = 0;
            Pawns = 0;
            PawnsPassed = 0;
            ShelterStorm = 0;
            StageStartWeight = ScaleFactor.FULL;
            ScaleWhite = ScaleFactor.FULL;
            ScaleBlack = ScaleFactor.FULL;
            DrawScore = 0;
            PassedPawns = Bitboard.Empty;
            CandidatePawns = Bitboard.Empty;
            LazyAge = -1;
        } 

        public void MaterialPawnsApply(Board board, MaterialResults material, PawnResults pawns, int drawScore)
        {
            this.Zobrist = board.ZobristBoard;
            this.DrawScore = drawScore;
            this.PcSq = board.PcSqValue;
            this.Material = material.Score;
            this.StageStartWeight = material.StartWeight;
            this.ScaleWhite = material.ScaleWhite;
            this.ScaleBlack = material.ScaleBlack;
            this.Pawns = pawns.Value;
            this.PassedPawns = pawns.PassedPawns;
            this.CandidatePawns = pawns.Candidates;
        }

        public void ApplyPreviousEval(Board board, EvalResults prev)
        {
            System.Diagnostics.Debug.Assert(prev != null);

            //age of this lazy eval is one greater than previous.
            this.LazyAge = prev.LazyAge + 1;
            
            //copy advanced evaluation items.
            this.Attacks[0].KingAttackerScore = prev.Attacks[0].KingAttackerScore;
            this.Attacks[1].KingAttackerScore = prev.Attacks[1].KingAttackerScore;
            this.Attacks[0].Mobility = prev.Attacks[0].Mobility;
            this.Attacks[1].Mobility = prev.Attacks[1].Mobility;
            this.PawnsPassed = prev.PawnsPassed;
            this.ShelterStorm = prev.ShelterStorm;

        }

        public int LazyHigh
        {
            get
            {
                int margin = LazyAge * 50;
                return Score + margin;
            }
        }
        public int LazyLow
        {
            get
            {
                int margin = LazyAge * 50;
                return Score - margin;
            }
        }

        public int Score
        {
            get
            {
                int nonScaled = PcSq
                    .Add(Pawns)
                    .Add(PawnsPassed)
                    .Add(ShelterStorm)
                    .Add(this.Attacks[0].Mobility.Subtract(this.Attacks[1].Mobility)).ApplyScaleFactor(StageStartWeight) + Material;

                nonScaled += this.Attacks[0].KingAttackerScore;
                nonScaled -= this.Attacks[1].KingAttackerScore;

                if (nonScaled > DrawScore && ScaleWhite < ScaleFactor.FULL)
                {
                    return ScaleWhite.ScaleValue(nonScaled - DrawScore) + DrawScore;
                }
                else if (nonScaled < DrawScore && ScaleBlack < ScaleFactor.FULL)
                {
                    return ScaleBlack.ScaleValue(nonScaled - DrawScore) + DrawScore;
                }
                return nonScaled;
            }
        }

        public int PositionalScore
        {
            get
            {
                return Score - Material;
            }
        }

        public int PcSqPhased
        {
            get
            {
                return PcSq.ApplyScaleFactor(StageStartWeight);
            }
        }

        public int MobilityPhased
        {
            get
            {
                return Attacks[0].Mobility.Subtract(Attacks[1].Mobility).ApplyScaleFactor(StageStartWeight);
            }
        }
        public int PawnsPhased
        {
            get
            {
                return Pawns.ApplyScaleFactor(StageStartWeight);
            }
        }

        public int PawnsPassedPhased
        {
            get
            {
                return PawnsPassed.ApplyScaleFactor(StageStartWeight);
            }
        }

        public int KingSafetyPhased
        {
            get
            {
                return Attacks[0].KingAttackerScore - Attacks[1].KingAttackerScore;
            }
        }


    }

}
