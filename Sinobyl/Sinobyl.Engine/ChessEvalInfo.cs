using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public class ChessEvalInfo
    {

        public enum EvalState
        {
            Initialized,
            Lazy,
            Full
        }

        public EvalState State { get; set; }
        public int[] Workspace = new int[64];
        public ChessEvalAttackInfo[] Attacks = new ChessEvalAttackInfo[] { new ChessEvalAttackInfo(), new ChessEvalAttackInfo() };

        public int Material { get; private set; }

        public PhasedScore PcSqStart { get; set; }

        public PhasedScore PawnsStart { get; private set; }

        public PhasedScore PawnsPassedStart = 0;

        public PhasedScore ShelterStorm = 0;
        public int StageStartWeight { get; private set; }
        public int ScaleWhite { get; private set; }
        public int ScaleBlack { get; private set; }
        public int DrawScore = 0;

        public ChessBitboard PassedPawns { get; private set; }
        public ChessBitboard CandidatePawns { get; private set; }

        public void Reset()
        {
            State = EvalState.Initialized;
            Attacks[0].Reset();
            Attacks[1].Reset();
            Material = 0;
            PcSqStart = 0;
            PawnsStart = 0;
            PawnsPassedStart = 0;
            ShelterStorm = 0;
            StageStartWeight = 0;
            ScaleWhite = 100;
            ScaleBlack = 100;
            DrawScore = 0;
            PassedPawns = ChessBitboard.Empty;
            CandidatePawns = ChessBitboard.Empty;
        }

        public void MaterialPawnsApply(ChessBoard board, EvalMaterialResults material, PawnInfo pawns)
        {
            this.State = EvalState.Lazy;
            this.PcSqStart = board.PcSqValue;
            this.Material = material.Score;
            this.StageStartWeight = material.StartWeight;
            this.ScaleWhite = material.ScaleWhite;
            this.ScaleBlack = material.ScaleBlack;
            this.PawnsStart = pawns.Value;
            this.PassedPawns = pawns.PassedPawns;
            this.CandidatePawns = pawns.Candidates;
        }

        public int LazyScore(ChessEvalInfo prevEvalInfo)
        {

            int nonScaled = PcSqStart
                .Add(PawnsStart)
                .Add(prevEvalInfo.PawnsPassedStart)
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
                int nonScaled = PcSqStart
                    .Add(PawnsStart)
                    .Add(PawnsPassedStart)
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

        public int PcSq
        {
            get
            {
                return PcSqStart.ApplyWeights(StageStartWeight);
            }
        }

        public int Mobility
        {
            get
            {
                return Attacks[0].Mobility.Subtract(Attacks[1].Mobility).ApplyWeights(StageStartWeight);
            }
        }
        public int Pawns
        {
            get
            {
                return PawnsStart.ApplyWeights(StageStartWeight);
            }
        }

        public int PawnsPassed
        {
            get
            {
                return PawnsPassedStart.ApplyWeights(StageStartWeight);
            }
        }


    }
}
