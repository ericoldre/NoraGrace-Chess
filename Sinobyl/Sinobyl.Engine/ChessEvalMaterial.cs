using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public interface IChessEvalMaterial
    {
        EvalMaterialResults EvalMaterialHash(ChessBoard board);
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

        public EvalMaterialResults EvalMaterialHash(ChessBoard board)
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
                wp: board.PieceCount(ChessPiece.WPawn),
                wn: board.PieceCount(ChessPiece.WKnight),
                wb: board.PieceCount(ChessPiece.WBishop),
                wr: board.PieceCount(ChessPiece.WRook),
                wq: board.PieceCount(ChessPiece.WQueen),
                bp: board.PieceCount(ChessPiece.BPawn),
                bn: board.PieceCount(ChessPiece.BKnight),
                bb: board.PieceCount(ChessPiece.BBishop),
                br: board.PieceCount(ChessPiece.BRook),
                bq: board.PieceCount(ChessPiece.BQueen));

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
            return new EvalMaterialResults(zob, startWeight, score);
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

        public EvalMaterialResults(Int64 zobristMaterial, int startWeight, int score /*, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq*/)
        {
            ZobristMaterial = zobristMaterial;
            StartWeight = startWeight;
            Score = score;

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

            double diff = white - black;

            double endgameMaterialBonus = 0;// (1f - totalPct) * .2f;

            double adjusted = diff * (1 + endgameMaterialBonus);

            return new EvalMaterialResults(zob, startWeight, (int)adjusted);

        }

        private double MyMaterial(double totalPct, double pawnPct, double minorPct, int myP, int myN, int myB, int myR, int myQ, int hisP, int hisN, int hisB, int hisR, int hisQ)
        {
            double pawnVal = CalcWeight(totalPct, _settings.MaterialValues[ChessPieceType.Pawn][ChessGameStage.Opening], _settings.MaterialValues[ChessPieceType.Pawn][ChessGameStage.Endgame]);
            double knightVal = CalcWeight(pawnPct, _settings.MaterialValues[ChessPieceType.Knight][ChessGameStage.Opening], _settings.MaterialValues[ChessPieceType.Knight][ChessGameStage.Endgame]);
            double bishopVal = CalcWeight(pawnPct, _settings.MaterialValues[ChessPieceType.Bishop][ChessGameStage.Opening], _settings.MaterialValues[ChessPieceType.Bishop][ChessGameStage.Endgame]);
            double rookVal = CalcWeight(pawnPct, _settings.MaterialValues[ChessPieceType.Rook][ChessGameStage.Opening], _settings.MaterialValues[ChessPieceType.Rook][ChessGameStage.Endgame]);
            double queenVal = CalcWeight(minorPct, _settings.MaterialValues[ChessPieceType.Queen][ChessGameStage.Opening], _settings.MaterialValues[ChessPieceType.Queen][ChessGameStage.Endgame]);
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
