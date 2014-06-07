using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public interface IChessEvalMaterial
    {
        EvalMaterialResults EvalMaterialHash(Board board);
    }
    public class ChessEvalMaterialBasic: IChessEvalMaterial
    {
        protected readonly ChessEvalSettings _settings;
        private readonly EvalMaterialResults[] _hash = new EvalMaterialResults[500];
        public static int TotalEvalMaterialCount = 0;

        public ChessEvalMaterialBasic(ChessEvalSettings settings)
        {
            _settings = settings;
        }

        public EvalMaterialResults EvalMaterialHash(Board board)
        {
            long idx = board.ZobristMaterial % _hash.GetUpperBound(0);

            if (idx < 0) { idx = -idx; }

            EvalMaterialResults retval = _hash[idx];
            if (retval != null && retval.ZobristMaterial == board.ZobristMaterial)
            {
                return retval;
            }

            retval = EvalMaterial(
                zob: board.ZobristMaterial,
                wp: board.PieceCount(Piece.WPawn),
                wn: board.PieceCount(Piece.WKnight),
                wb: board.PieceCount(Piece.WBishop),
                wr: board.PieceCount(Piece.WRook),
                wq: board.PieceCount(Piece.WQueen),
                bp: board.PieceCount(Piece.BPawn),
                bn: board.PieceCount(Piece.BKnight),
                bb: board.PieceCount(Piece.BBishop),
                br: board.PieceCount(Piece.BRook),
                bq: board.PieceCount(Piece.BQueen));

            _hash[idx] = retval;
            return retval;
        }

        public virtual EvalMaterialResults EvalMaterial(Int64 zob, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq)
        {
            TotalEvalMaterialCount++;

            int basicCount = 
                (wn * 3) + (bn * 3)
                + (wb * 3) + (bb * 3)
                + (wr * 5) + (br * 5)
                + (wq * 9) + (bq * 9);

            

            int startScore = (wp * 100)
                + (wn * 300)
                + (wb * 300)
                + (wr * 500)
                + (wq * 900)
                - (bp * 100)
                - (bn * 300)
                - (bb * 300)
                - (br * 500)
                - (bq * 900);

            int endScore = startScore;

            if (wb > 1)
            {
                startScore += 50;
                endScore += 50;
            }
            if (bb > 1)
            {
                startScore -= 50;
                endScore -= 50;
            }

            int startWeight = CalcStartWeight(wp, wn, wb, wr, wq, bp, bn, bb, br, bq);
            //int score = (int)(((float)startScore * startWeight) + ((float)endScore * (1 - startWeight)));
            int score = PhasedScoreInfo.Create(startScore, endScore).ApplyWeights(startWeight);
            return new EvalMaterialResults(zob, startWeight, score, 100, 100);
            //return new Results(zob, startWeight, startScore, endScore, basicCount, wp,  wn,  wb,  wr,  wq,  bp,  bn,  bb,  br,  bq);
        }

        protected virtual int CalcStartWeight(int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq)
        {
            int basicMaterialCount =
                (wn * 3) + (bn * 3)
                + (wb * 3) + (bb * 3)
                + (wr * 5) + (br * 5)
                + (wq * 9) + (bq * 9);

            //full material would be 62
            if (basicMaterialCount >= 56)
            {
                return 100;
            }
            else if (basicMaterialCount <= 10)
            {
                return 0;
            }
            else
            {
                int rem = basicMaterialCount - 10;
                float retval = (float)rem / 46f;
                int retval2 = (int)Math.Round((retval * 100));
                return retval2;
            }
        }
        


    }

    public class EvalMaterialResults
    {
        public readonly Int64 ZobristMaterial;
        public readonly int StartWeight;
        public readonly int Score;

        public readonly int ScaleWhite;
        public readonly int ScaleBlack;
        //public readonly int Wp;
        //public readonly int Wn;
        //public readonly int Wb;
        //public readonly int Wr;
        //public readonly int Wq;
        //public readonly int Bp;
        //public readonly int Bn;
        //public readonly int Bb;
        //public readonly int Br;
        //public readonly int Bq;

        public EvalMaterialResults(Int64 zobristMaterial, int startWeight, int score, int scaleWhite, int scaleBlack /*, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq*/)
        {
            ZobristMaterial = zobristMaterial;
            StartWeight = startWeight;
            Score = score;
            ScaleWhite = scaleWhite;
            ScaleBlack = scaleBlack;
            //Wp = wp;
            //Wn = wn;
            //Wb = wb;
            //Wr = wr;
            //Wq = wq;
            //Bp = bp;
            //Bn = bn;
            //Bb = bb;
            //Br = br;
            //Bq = bq;

        }

        public bool DoShelter
        {
            get { return this.StartWeight > 40; }
        }

    }

    public class ChessEvalMaterial2 : ChessEvalMaterialBasic
    {
        
        public ChessEvalMaterial2(ChessEvalSettings settings)
            : base(settings)
        {
        }


        public override EvalMaterialResults EvalMaterial(Int64 zob, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq)
        {
            TotalEvalMaterialCount++;





            int startWeight = CalcStartWeight(wp, wn, wb, wr, wq, bp, bn, bb, br, bq);
            double startWeightPct = (double)startWeight / 100f;

            
            double pctPawns = (double)(wp + bp) / 16f;
            double pctMinors = (double)(wn + bn + wb + bb) / 8;
            double totalPct = (startWeightPct + pctPawns) / 2f;

            double white = MyMaterial(totalPct, pctPawns, pctMinors, wp, wn, wb, wr, wq, bp, bn, bb, br, bq);
            double black = MyMaterial(totalPct, pctPawns, pctMinors, bp, bn, bb, br, bq, wp, wn, wb, wr, wq);

            double score = white - black;

            double scaleWhite = ScaleFactor(wp, wn, wb, wr, wq, bp, bn, bb, br, bq);
            double scaleBlack = ScaleFactor(bp, bn, bb, br, bq, wp, wn, wb, wr, wq);
            return new EvalMaterialResults(zob, startWeight, (int)score, (int)(scaleWhite * 100), (int)(scaleBlack * 100));

        }

        /// <summary>
        /// Functionally equivilent to Fruit2.1, although surprised adding it did not improve much.
        /// </summary>
        /// <returns></returns>
        private double ScaleFactor(int myP, int myN, int myB, int myR, int myQ, int hisP, int hisN, int hisB, int hisR, int hisQ)
        {
            if (myP == 0)
            { // white has no pawns

                int myMaj = myQ * 2 + myR;
                int myMin = myB + myN;
                int myTot = myMaj * 2 + myMin;

                int hisMaj = hisQ * 2 + hisR;
                int hisMin = hisB + hisN;
                int hisTot = hisMaj * 2 + hisMin;

                if (myTot == 1)
                {
                    System.Diagnostics.Debug.Assert(myMaj == 0);
                    System.Diagnostics.Debug.Assert(myMin == 1);
                    // KBK* or KNK*, always insufficient
                    return 0;
                }
                else if (myTot == 2 && myN == 2)
                {

                    System.Diagnostics.Debug.Assert(myMaj == 0);
                    System.Diagnostics.Debug.Assert(myMin == 2);

                    // KNNK*, usually insufficient

                    if (hisTot != 0 || hisP == 0)
                    {
                        return 0;
                    }
                    else
                    { // KNNKP+, might not be draw
                        return .1f;
                    }

                }
                else if (myTot == 2 && myB == 2 && hisTot == 1 && hisN == 1)
                {

                    System.Diagnostics.Debug.Assert(myMaj == 0);
                    System.Diagnostics.Debug.Assert(myMin == 2);
                    System.Diagnostics.Debug.Assert(hisMaj == 0);
                    System.Diagnostics.Debug.Assert(hisMin == 1);

                    // KBBKN*, barely drawish (not at all?)

                    return .5;

                }
                else if (myTot - hisTot <= 1 && myMaj <= 2)
                {

                    // no more than 1 minor up, drawish

                    return .15;
                }

            }
            else if (myP == 1)
            { // white has one pawn

                int w_maj = myQ * 2 + myR;
                int w_min = myB + myN;
                int w_tot = w_maj * 2 + w_min;

                int b_maj = hisQ * 2 + hisR;
                int b_min = hisB + hisN;
                int b_tot = b_maj * 2 + b_min;

                if (b_min != 0)
                {

                    // assume black sacrifices a minor against the lone pawn

                    b_min--;
                    b_tot--;

                    if (w_tot == 1)
                    {

                        System.Diagnostics.Debug.Assert(w_maj == 0);
                        System.Diagnostics.Debug.Assert(w_min == 1);

                        // KBK* or KNK*, always insufficient
                        return .25;

                    }
                    else if (w_tot == 2 && myN == 2)
                    {

                        System.Diagnostics.Debug.Assert(w_maj == 0);
                        System.Diagnostics.Debug.Assert(w_min == 2);

                        // KNNK*, usually insufficient
                        return .25f;
                        //mul[White] = 4; // 1/4

                    }
                    else if (w_tot - b_tot <= 1 && w_maj <= 2)
                    {

                        // no more than 1 minor up, drawish
                        return .5f;
                        //mul[White] = 8; // 1/2
                    }

                }
                else if (hisR != 0)
                {

                    // assume black sacrifices a rook against the lone pawn

                    b_maj--;
                    b_tot -= 2;

                    if (w_tot == 1)
                    {

                        System.Diagnostics.Debug.Assert(w_maj == 0);
                        System.Diagnostics.Debug.Assert(w_min == 1);

                        // KBK* or KNK*, always insufficient
                        return .25f;
                        //mul[White] = 4; // 1/4

                    }
                    else if (w_tot == 2 && myN == 2)
                    {

                        System.Diagnostics.Debug.Assert(w_maj == 0);
                        System.Diagnostics.Debug.Assert(w_min == 2);

                        // KNNK*, usually insufficient
                        return .25f;
                        //mul[White] = 4; // 1/4

                    }
                    else if (w_tot - b_tot <= 1 && w_maj <= 2)
                    {

                        // no more than 1 minor up, drawish
                        return .5f;
                        //mul[White] = 8; // 1/2
                    }
                }
            }

            return 1;

        }


        private double MyMaterial(double totalPct, double pawnPct, double minorPct, int myP, int myN, int myB, int myR, int myQ, int hisP, int hisN, int hisB, int hisR, int hisQ)
        {
            double pawnVal = CalcWeight(totalPct, _settings.MaterialValues[PieceType.Pawn][ChessGameStage.Opening], _settings.MaterialValues[PieceType.Pawn][ChessGameStage.Endgame]);
            double knightVal = CalcWeight(pawnPct, _settings.MaterialValues[PieceType.Knight][ChessGameStage.Opening], _settings.MaterialValues[PieceType.Knight][ChessGameStage.Endgame]);
            double bishopVal = CalcWeight(pawnPct, _settings.MaterialValues[PieceType.Bishop][ChessGameStage.Opening], _settings.MaterialValues[PieceType.Bishop][ChessGameStage.Endgame]);
            double rookVal = CalcWeight(pawnPct, _settings.MaterialValues[PieceType.Rook][ChessGameStage.Opening], _settings.MaterialValues[PieceType.Rook][ChessGameStage.Endgame]);
            double queenVal = CalcWeight(minorPct, _settings.MaterialValues[PieceType.Queen][ChessGameStage.Opening], _settings.MaterialValues[PieceType.Queen][ChessGameStage.Endgame]);
            double bishopPairValue = CalcWeight(pawnPct, _settings.MaterialBishopPair.Opening, _settings.MaterialBishopPair.Endgame);

            

            //if (hisQ > myQ)
            //{
            //    knightVal *= 1.08f;
            //}

            double retval = 0;
            retval += (myP * pawnVal);
            retval += (myN * knightVal);
            retval += (myB * bishopVal);
            retval += (myR * rookVal);
            retval += (myQ * queenVal);

            if (myB > 1)
            {
                retval += bishopPairValue;
            }
            
            ////would like to have at least 1 of knight, bishop, rook
            //if (myN > 0) { retval += (knightVal * .05f); }
            //if (myB > 0) { retval += (bishopVal * .05f); }
            //if (myR > 0) { retval += (rookVal * .05f); }


            return retval;
        }

        private double CalcWeight(double startWeight, double startScore, double endScore)
        {
            System.Diagnostics.Debug.Assert(startWeight >= 0 && startWeight <= 1);
            return (startScore * startWeight) + (endScore * (1f - startWeight));
        }
    }
}
