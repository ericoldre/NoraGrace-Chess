using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine.Evaluation
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
        public int StageStartWeight { get; private set; }
        public int ScaleWhite { get; private set; }
        public int ScaleBlack { get; private set; }
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
            StageStartWeight = 0;
            ScaleWhite = 100;
            ScaleBlack = 100;
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

        public int KingSafetyPhased
        {
            get
            {
                return Attacks[0].KingAttackerScore - Attacks[1].KingAttackerScore;
            }
        }


    }

    public class ChessEvalInfoStack
    {

        public readonly Evaluator _eval;

        List<EvalResults> _plyInfoList = new List<EvalResults>();

        public ChessEvalInfoStack(Evaluator eval, int plyCapacity = 50)
        {
            _eval = eval;
            while (_plyInfoList.Count < plyCapacity)
            {
                _plyInfoList.Add(new EvalResults());
            }
        }

        public Evaluator Evaluator
        {
            get { return _eval; }
        }

        public int EvalFor(int ply, Board board, Player player, out EvalResults info, int alpha, int beta)
        {
            System.Diagnostics.Debug.Assert(alpha >= Evaluator.MinValue);
            System.Diagnostics.Debug.Assert(beta <= Evaluator.MaxValue);

            if (player == Player.White)
            {
                return Eval(ply, board, out info, alpha, beta);
            }
            else
            {
                return -Eval(ply, board, out info, -beta, -alpha);
            }
        }

        public int Eval(int ply, Board board, out EvalResults info, int alpha, int beta)
        {
            System.Diagnostics.Debug.Assert(alpha >= Evaluator.MinValue);
            System.Diagnostics.Debug.Assert(beta <= Evaluator.MaxValue);

            info = _plyInfoList[ply];


            //check to see if we already have evaluated.
            if(board.ZobristBoard == info.Zobrist)
            {
                if (info.LazyAge == 0) { return info.Score; }
                if (info.LazyHigh < alpha) 
                {
                    return info.LazyHigh; 
                }
                else if (info.LazyLow > beta) 
                {
                    return info.LazyLow; 
                }
            }

            
            EvalResults prev = null;
            if (ply > 0)
            {
                prev = _plyInfoList[ply - 1];
                if (prev.Zobrist != board.ZobristPrevious)
                {
                    prev = null;
                }
            }

            return _eval.EvalLazy(board, info, prev, alpha, beta);
            
        }



    }
}
