using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public class ChessEvalInfo
    {

        public int[] Workspace = new int[64];


        //basic eval terms:
        public long Zobrist { get; private set; }
        public int DrawScore { get; private set; }
        public int Material { get; private set; }
        public PhasedScore PcSq { get; set; }
        public PhasedScore Pawns { get; private set; }
        public int StageStartWeight { get; private set; }
        public int ScaleWhite { get; private set; }
        public int ScaleBlack { get; private set; }
        public ChessBitboard PassedPawns { get; private set; }   //not a score but information
        public ChessBitboard CandidatePawns { get; private set; }  //not a score but information

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
            StageStartWeight = 0;
            ScaleWhite = 100;
            ScaleBlack = 100;
            DrawScore = 0;
            PassedPawns = ChessBitboard.Empty;
            CandidatePawns = ChessBitboard.Empty;
            LazyAge = -1;
        } 

        public void MaterialPawnsApply(ChessBoard board, EvalMaterialResults material, PawnInfo pawns, int drawScore)
        {
            this.Zobrist = board.Zobrist;
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

        public void ApplyPreviousEval(ChessBoard board, ChessEvalInfo prev)
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

        public int LazyScore(ChessEvalInfo prevEvalInfo)
        {

            int nonScaled = PcSq
                .Add(Pawns)
                .Add(prevEvalInfo.PawnsPassed)
                .Add(prevEvalInfo.ShelterStorm)
                .Add(prevEvalInfo.Attacks[0].Mobility.Subtract(prevEvalInfo.Attacks[1].Mobility)).ApplyWeights(StageStartWeight) + Material;

            nonScaled += prevEvalInfo.Attacks[0].KingAttackerScore;
            nonScaled -= prevEvalInfo.Attacks[1].KingAttackerScore;

            if (nonScaled > DrawScore && ScaleWhite < 100)
            {
                int scaled = (((nonScaled - DrawScore) * ScaleWhite) / 100) + DrawScore;
                return scaled;
            }
            else if (nonScaled < DrawScore && ScaleBlack < 100)
            {
                int scaled = (((nonScaled - DrawScore) * ScaleBlack) / 100) + DrawScore;
                return scaled;
            }
            return nonScaled;
        }

        public int StageEndWeight
        {
            get { return 100 - StageStartWeight; }
        }

        public int Score
        {
            get
            {
                int nonScaled = PcSq
                    .Add(Pawns)
                    .Add(PawnsPassed)
                    .Add(ShelterStorm)
                    .Add(this.Attacks[0].Mobility.Subtract(this.Attacks[1].Mobility)).ApplyWeights(StageStartWeight) + Material;

                nonScaled += this.Attacks[0].KingAttackerScore;
                nonScaled -= this.Attacks[1].KingAttackerScore;

                if (nonScaled > DrawScore && ScaleWhite < 100)
                {
                    int scaled = (((nonScaled - DrawScore) * ScaleWhite) / 100) + DrawScore;
                    return scaled;
                }
                else if (nonScaled < DrawScore && ScaleBlack < 100)
                {
                    int scaled = (((nonScaled - DrawScore) * ScaleBlack) / 100) + DrawScore;
                    return scaled;
                }
                return nonScaled;
            }
        }

        public int PcSqPhased
        {
            get
            {
                return PcSq.ApplyWeights(StageStartWeight);
            }
        }

        public int MobilityPhased
        {
            get
            {
                return Attacks[0].Mobility.Subtract(Attacks[1].Mobility).ApplyWeights(StageStartWeight);
            }
        }
        public int PawnsPhased
        {
            get
            {
                return Pawns.ApplyWeights(StageStartWeight);
            }
        }

        public int PawnsPassedPhased
        {
            get
            {
                return PawnsPassed.ApplyWeights(StageStartWeight);
            }
        }


    }
}
