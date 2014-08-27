using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine.Evaluation.Helpers;

namespace NoraGrace.Engine.Evaluation
{

    public class MaterialSettings : ChessGameStageDictionary<MaterialSettingsPhase>
    {
        

    }
    public class MaterialSettingsPhase
    {

        public class PieceSettings
        {
            public int BaseValue { get; set; }
            public int PairBonus { get; set; }
        }

        public class PawnSettings
        {
            public int First { get; set; }
            public int Eighth { get; set; }
            public Point Curve { get; set; }
            public PawnSettings()
            {
                Curve = new Helpers.Point();
            }

            public int[] GetValues()
            {
                var vals = QuadBezierCurve.GetIntegerValues(7, First, Eighth, Curve.X, Curve.Y);
                return vals.Select(d => (int)d).ToArray();
            }
        }

        public PawnSettings Pawn { get; set; }
        public PieceSettings Knight { get; set; }
        public PieceSettings Bishop { get; set; }
        public PieceSettings Rook { get; set; }
        public PieceSettings Queen { get; set; }

        public MaterialSettingsPhase()
        {
            Pawn = new PawnSettings();
            Knight = new PieceSettings();
            Bishop = new PieceSettings();
            Rook = new PieceSettings();
            Queen = new PieceSettings();
        }

    }

    public class MaterialResults
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

        public MaterialResults(Int64 zobristMaterial, int startWeight, int score, int scaleWhite, int scaleBlack /*, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq*/)
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

    public class MaterialEvaluator
    {

        protected readonly MaterialSettings _settings;
        private readonly MaterialResults[] _hash = new MaterialResults[500];
        public static int TotalEvalMaterialCount = 0;

        private int[] _pawnValuesOpening;
        private int[] _pawnValuesEndGame;
        public MaterialEvaluator(MaterialSettings settings)
        {
            _settings = settings;

            _pawnValuesOpening = settings.Opening.Pawn.GetValues();
            _pawnValuesEndGame = settings.Endgame.Pawn.GetValues();
        }

        public MaterialResults EvalMaterialHash(Board board)
        {
            long idx = board.ZobristMaterial % _hash.GetUpperBound(0);

            if (idx < 0) { idx = -idx; }

            MaterialResults retval = _hash[idx];
            if (retval != null && retval.ZobristMaterial == board.ZobristMaterial)
            {
                return retval;
            }

            retval = EvalMaterial(
                zob: board.ZobristMaterial,
                wp: board.PieceCount(Player.White, PieceType.Pawn),
                wn: board.PieceCount(Player.White, PieceType.Knight),
                wb: board.PieceCount(Player.White, PieceType.Bishop),
                wr: board.PieceCount(Player.White, PieceType.Rook),
                wq: board.PieceCount(Player.White, PieceType.Queen),
                bp: board.PieceCount(Player.Black, PieceType.Pawn),
                bn: board.PieceCount(Player.Black, PieceType.Knight),
                bb: board.PieceCount(Player.Black, PieceType.Bishop),
                br: board.PieceCount(Player.Black, PieceType.Rook),
                bq: board.PieceCount(Player.Black, PieceType.Queen));

            _hash[idx] = retval;
            return retval;
        }


        public MaterialResults EvalMaterial(Int64 zob, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq)
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


            return new MaterialResults(zob, startWeight, (int)score, (int)(scaleWhite * 100), (int)(scaleBlack * 100));

        }



        protected int CalcStartWeight(int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq)
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

            double pawnsOpening = _pawnValuesOpening.Take(myP).Sum();
            double pawnsEndgame = _pawnValuesEndGame.Take(myP).Sum();

            double knightVal = CalcWeight(pawnPct, _settings.Opening.Knight.BaseValue, _settings.Endgame.Knight.BaseValue);
            double bishopVal = CalcWeight(pawnPct, _settings.Opening.Bishop.BaseValue, _settings.Endgame.Bishop.BaseValue);
            double rookVal = CalcWeight(pawnPct, _settings.Opening.Rook.BaseValue, _settings.Endgame.Rook.BaseValue);
            double queenVal = CalcWeight(minorPct, _settings.Opening.Queen.BaseValue, _settings.Endgame.Queen.BaseValue);

            double bishopPairValue = CalcWeight(pawnPct, _settings.Opening.Bishop.PairBonus, _settings.Endgame.Bishop.PairBonus);

            

            //if (hisQ > myQ)
            //{
            //    knightVal *= 1.08f;
            //}

            double retval = CalcWeight(totalPct, pawnsOpening, pawnsEndgame);
            //retval += (myP * pawnVal);
            retval += (myN * knightVal);
            retval += (myB * bishopVal);
            retval += (myR * rookVal);
            retval += (myQ * queenVal);

            if (myN > 1)
            {
                retval += CalcWeight(pawnPct, _settings.Opening.Knight.PairBonus, _settings.Endgame.Knight.PairBonus); ;
            }

            if (myB > 1)
            {
                retval += CalcWeight(pawnPct, _settings.Opening.Bishop.PairBonus, _settings.Endgame.Bishop.PairBonus); ;
            }

            if (myR > 1)
            {
                retval += CalcWeight(pawnPct, _settings.Opening.Rook.PairBonus, _settings.Endgame.Rook.PairBonus); ;
            }

            if (myQ > 1)
            {
                retval += CalcWeight(minorPct, _settings.Opening.Queen.PairBonus, _settings.Endgame.Queen.PairBonus); ;
            }
            
            ////would like to have at least 1 of knight, bishop, rook
            //if (myN > 0) { retval += (knightVal * .05f); }
            //if (myB > 0) { retval += (bishopVal * .05f); }
            //if (myR > 0) { retval += (rookVal * .05f); }


            return retval;
        }

        private double CalcWeight(double startWeight, double startScore, double endScore)
        {
            startWeight = Math.Min(1, Math.Max(0, startWeight));
            System.Diagnostics.Debug.Assert(startWeight >= 0 && startWeight <= 1);
            return (startScore * startWeight) + (endScore * (1f - startWeight));
        }
    }
}
