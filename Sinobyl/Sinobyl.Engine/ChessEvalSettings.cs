using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{

    public class ChessEvalSettings
    {
        #region Piece Square Tables
        public int[] PcSqPawnStart = new int[]{
			0,  0,  0,  0,  0,  0,  0,  0,	
			-10, -5, 0, 5,  5,  0, -5, -10,
			-10, -5, 0, 5,  5,  0, -5, -10,	
			-10, -5, 0, 10, 10, 0, -5, -10,	
			-10, -5, 5, 15, 15, 0, -5, -10,	
			-10, -5, 5, 10, 10, 0, -5, -10,	
			-10, -5, 0, 5,  5,  5, -5, -10,	
			 0,  0,  0,  0,  0,  0,  0,  0,	
		};
        public int[] PcSqPawnEnd = new int[] { 
			0,  0,  0,  0,  0,  0,  0,  0,	
			20, 25, 34, 38, 38, 34, 25, 20,
			12, 20, 24, 32, 32, 24, 20, 12,	
			 6, 11, 18,	27, 27, 16,	11,  6,	
			 4,  7, 10, 12, 12, 10,  7,  4,	
			-3, -3, -3, -3, -3, -3, -3,  -3,	
			-10,-10,-10,-10,-10,-10,-10,-10,	
			 0,  0,  0,  0,  0,  0,  0,  0,			
		};

        public int[] PcSqKnightStart = new int[] { 
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,			
		};

        public int[] PcSqKnightEnd = new int[] { 
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,				
		};

        public int[] PcSqBishopStart = new int[] { 
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			-3,	 9,	11,	11,	11,	11,	 9,	-3,	
			-3,	 8,	12,	12,	12,	12,	 8,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,			
		};


        public int[] PcSqBishopEnd = new int[] { 
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			-3,	 9,	11,	11,	11,	11,	 9,	-3,	
			-3,	 8,	12,	12,	12,	12,	 8,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,					
		};

        public int[] PcSqRookStart = new int[] { 
			0,	0,	4,	6,	6,	4,	0,	0,	
			10,	10,	14,	16,	16,	14,	10,	10,
			0,	0,	4,	6,	6,	4,	0,	0,
			0,	0,	4,	6,	6,	4,	0,	0,
			0,	0,	4,	6,	6,	4,	0,	0,	
			0,	0,	4,	6,	6,	4,	0,	0,	
			0,	0,	4,	6,	6,	4,	0,	0,	
			0,	0,	4,	6,	6,	4,	0,	0			
		};

        public int[] PcSqRookEnd = new int[] { 
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0			
		};

        public int[] PcSqQueenStart = new int[] { 
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,  0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	1,	1,	1,	1,	0,	0,	0,	
			0,	0,	1,	1,	1,	0,	0,	0,	
			-2,-2, -2,  0,	0, -2, -2, -2
		};

        public int[] PcSqQueenEnd = new int[] { 
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,  0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	1,	1,	1,	1,	0,	0,	0,	
			0,	0,	1,	1,	1,	0,	0,	0,	
			-2,-2, -2,  0,	0, -2, -2, -2		
		};

        public int[] PcSqKingStart = new int[] { 
			-80,-80,-80,-80,-80,-80,-80,-80,
			-80,-80,-80,-80,-80,-80,-80,-80,
			-80,-80,-80,-80,-80,-80,-80,-80,
			-60,-60,-60,-60,-60,-60,-60,-60,
			-40,-40,-40,-40,-40,-40,-40,-40,
			-7,	-15,-15,-15,-15,-15,-15,-7,	
			-5,	-5,	-12,-12,-12,-12,-5,	-5,	
			 3,	 3,	 8,	-5,  -8,-5, 10,	 5		
		};

        public int[] PcSqKingEnd = new int[] { 
			 -8, -8, -8, -8, -8, -8, -8, -8,
			 -8, -0, -0, -0, -0, -0, -0, -8,
			 -8, -0, 05, 05, 05, 05, -0, -8,
			 -8, -0, 05, 30, 30, 05, -0, -8,
			 -8, -0, 05, 30, 30, 05, -0, -8,
			 -8, -0, 05, 30, 30, 05, -0, -8,
			 -8, -0, -0, -0, -0, -0, -0, -8,
			 -8, -8, -8, -8, -8, -8, -8, -8,		
		};
        #endregion

        #region pawnsetup
        public int PawnDoubledStart = 10;
        public int PawnDoubledEnd = 25;
        public int PawnIsoStart = 15;
        public int PawnIsoEnd = 20;

        public int[] PawnPassedStart = new int[] { 
			  0,  0,  0,  0,  0,  0,  0,  0,
			 50, 50, 50, 50, 50, 50, 50, 50,
			 30, 30, 30, 30, 30, 30, 30, 30,
			 20, 20, 20, 20, 20, 20, 20, 20,
			 15, 15, 15, 15, 15, 15, 15, 15,
			 15, 15, 15, 15, 15, 15, 15, 15,
			 15, 15, 15, 15, 15, 15, 15, 15,
			  0,  0,  0,  0,  0,  0,  0,  0
		};
        public int[] PawnPassedEnd = new int[] { 
			  0,  0,  0,  0,  0,  0,  0,  0,
			140,140,140,140,140,140,140,140,
			 90, 90, 90, 90, 90, 90, 90, 90,
			 60, 60, 60, 60, 60, 60, 60, 60,
			 40, 40, 40, 40, 40, 40, 40, 40,
			 25, 25, 25, 25, 25, 25, 25, 25,
			 15, 15, 15, 15, 15, 15, 15, 15,
			  0,  0,  0,  0,  0,  0,  0,  0	
		};
        #endregion
        public int MatPawnStart = 100;
        public int MatPawnEnd = 100;
        public int MatKnightStart = 300;
        public int MatKnightEnd = 300;
        public int MatBishopStart = 300;
        public int MatBishopEnd = 300;
        public int MatRookStart = 500;
        public int MatRookEnd = 500;
        public int MatQueenStart = 900;
        public int MatQueenEnd = 900;

        public int MatBishopPairStart = 0;
        public int MatBishopPairEnd = 0;


        public override bool Equals(object obj)
        {
            ChessEvalSettings other = obj as ChessEvalSettings;
            if (other == null) { return false; }

            bool different = false;

            foreach (ChessPosition pos in Chess.AllPositions)
            {
                different |= this.PcSqPawnStart[pos.GetIndex64()] != other.PcSqPawnStart[pos.GetIndex64()];
                different |= this.PcSqPawnEnd[pos.GetIndex64()] != other.PcSqPawnEnd[pos.GetIndex64()];

                different |= this.PcSqKnightStart[pos.GetIndex64()] != other.PcSqKnightStart[pos.GetIndex64()];
                different |= this.PcSqKnightEnd[pos.GetIndex64()] != other.PcSqKnightEnd[pos.GetIndex64()];

                different |= this.PcSqBishopStart[pos.GetIndex64()] != other.PcSqKnightStart[pos.GetIndex64()];
                different |= this.PcSqBishopEnd[pos.GetIndex64()] != other.PcSqKnightEnd[pos.GetIndex64()];

                different |= this.PcSqRookStart[pos.GetIndex64()] != other.PcSqRookStart[pos.GetIndex64()];
                different |= this.PcSqRookEnd[pos.GetIndex64()] != other.PcSqRookEnd[pos.GetIndex64()];

                different |= this.PcSqQueenStart[pos.GetIndex64()] != other.PcSqQueenStart[pos.GetIndex64()];
                different |= this.PcSqQueenEnd[pos.GetIndex64()] != other.PcSqQueenEnd[pos.GetIndex64()];

                different |= this.PcSqKingStart[pos.GetIndex64()] != other.PcSqKingStart[pos.GetIndex64()];
                different |= this.PcSqKingEnd[pos.GetIndex64()] != other.PcSqKingEnd[pos.GetIndex64()];

                different |= this.PawnPassedStart[pos.GetIndex64()] != other.PawnPassedStart[pos.GetIndex64()];
                different |= this.PawnPassedEnd[pos.GetIndex64()] != other.PawnPassedEnd[pos.GetIndex64()];
            }
            different |= this.MatPawnStart != other.MatPawnStart;
            different |= this.MatPawnEnd != other.MatPawnEnd;
            different |= this.MatKnightStart != other.MatKnightStart;
            different |= this.MatKnightEnd != other.MatKnightEnd;
            different |= this.MatBishopStart != other.MatBishopStart;
            different |= this.MatBishopEnd != other.MatBishopEnd;
            different |= this.MatRookStart != other.MatRookStart;
            different |= this.MatRookEnd != other.MatRookEnd;
            different |= this.MatQueenStart != other.MatQueenStart;
            different |= this.MatQueenEnd != other.MatQueenEnd;

            different |= this.MatBishopPairStart != other.MatBishopPairStart;
            different |= this.MatBishopPairEnd != other.MatBishopPairEnd;

            different |= this.PawnDoubledStart != other.PawnDoubledStart;
            different |= this.PawnDoubledEnd != other.PawnDoubledEnd;




            return base.Equals(obj);
        }

    }

    //public class ChessEvalSettings
    //{

    //    //material
    //    public int MaterialPawnStart { get; set; }
    //    public int MaterialPawnEnd { get; set; }
    //    public int MaterialKnightStart { get; set; }
    //    public int MaterialKnightEnd { get; set; }
    //    public int MaterialBishopStart { get; set; }
    //    public int MaterialBishopEnd { get; set; }
    //    public int MaterialRookStart { get; set; }
    //    public int MaterialRookEnd { get; set; }
    //    public int MaterialQueenStart { get; set; }
    //    public int MaterialQueenEnd { get; set; }

    //    public int MaterialBishopPairStart { get; set; }
    //    public int MaterialBishopPairEnd { get; set; }

    //    //pcsq
    //    public int[] PcSqPawnStart { get; set; }
    //    public int[] PcSqPawnEnd { get; set; }
    //    public int[] PcSqKnightStart { get; set; }
    //    public int[] PcSqKnightEnd { get; set; }
    //    public int[] PcSqBishopStart { get; set; }
    //    public int[] PcSqBishopEnd { get; set; }
    //    public int[] PcSqRookStart { get; set; }
    //    public int[] PcSqRookEnd { get; set; }
    //    public int[] PcSqQueenStart { get; set; }
    //    public int[] PcSqQueenEnd { get; set; }
    //    public int[] PcSqKingStart { get; set; }
    //    public int[] PcSqKingEnd { get; set; }


    //    //pawn
    //    public int PawnDoubledStart { get; set; }
    //    public int PawnDoubledEnd { get; set; }
    //    public int PawnIsoStart { get; set; }
    //    public int PawnIsoEnd { get; set; }

    //    public int[] PawnPassedStart { get; set; }
    //    public int[] PawnPassedEnd { get; set; }

    //    public ChessEvalSettings()
    //    {
    //        PcSqPawnStart = new int[64];
    //        PcSqPawnEnd = new int[64];
    //        PcSqKnightStart = new int[64];
    //        PcSqKnightEnd = new int[64];
    //        PcSqBishopStart = new int[64];
    //        PcSqBishopEnd = new int[64];
    //        PcSqRookStart = new int[64];
    //        PcSqRookEnd = new int[64];
    //        PcSqQueenStart = new int[64];
    //        PcSqQueenEnd = new int[64];
    //        PcSqKingStart = new int[64];
    //        PcSqKingEnd = new int[64];

    //        PawnPassedStart = new int[64];
    //        PawnPassedEnd = new int[64];
    //    }

    //    public static ChessEvalSettings Default()
    //    {
    //        ChessEvalSettings retval = new ChessEvalSettings();

    //        retval.MaterialPawnStart = 100;
    //        retval.MaterialPawnEnd = 100;
    //        retval.MaterialKnightStart = 300;
    //        retval.MaterialKnightEnd = 300;
    //        retval.MaterialBishopStart = 300;
    //        retval.MaterialBishopEnd = 300;
    //        retval.MaterialRookStart = 500;
    //        retval.MaterialRookEnd = 500;
    //        retval.MaterialQueenStart = 900;
    //        retval.MaterialQueenEnd = 900;

    //        retval.MaterialBishopPairStart = 0;
    //        retval.MaterialBishopPairEnd = 0;

    //        retval.PcSqPawnStart = new int[]{
    //            0,  0,  0,  0,  0,  0,  0,  0,	
    //            -10, -5, 0, 5,  5,  0, -5, -10,
    //            -10, -5, 0, 5,  5,  0, -5, -10,	
    //            -10, -5, 0, 10, 10, 0, -5, -10,	
    //            -10, -5, 5, 15, 15, 0, -5, -10,	
    //            -10, -5, 5, 10, 10, 0, -5, -10,	
    //            -10, -5, 0, 5,  5,  5, -5, -10,	
    //             0,  0,  0,  0,  0,  0,  0,  0,	
    //        };

    //        retval.PcSqPawnEnd = new int[] { 
    //            0,  0,  0,  0,  0,  0,  0,  0,	
    //            20, 25, 34, 38, 38, 34, 25, 20,
    //            12, 20, 24, 32, 32, 24, 20, 12,	
    //             6, 11, 18,	27, 27, 16,	11,  6,	
    //             4,  7, 10, 12, 12, 10,  7,  4,	
    //            -3, -3, -3, -3, -3, -3, -3,  -3,	
    //            -10,-10,-10,-10,-10,-10,-10,-10,	
    //             0,  0,  0,  0,  0,  0,  0,  0,			
    //        };

    //        retval.PcSqKnightStart = new int[] { 
    //            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
    //             2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
    //             3,	 7,	14,	16,	16,	14,	 7,	 3,	
    //             4,	 9,	15,	16,	16,	15,	 9,	 4,	
    //             4,	 9,	13,	15,	15,	13,	 9,	 4,	
    //             3,	 7,	11,	13,	13,	11,	 7,	 3,	
    //             2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
    //            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,			
    //        };

    //        retval.PcSqKnightEnd = new int[] { 
    //            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
    //             2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
    //             3,	 7,	14,	16,	16,	14,	 7,	 3,	
    //             4,	 9,	15,	16,	16,	15,	 9,	 4,	
    //             4,	 9,	13,	15,	15,	13,	 9,	 4,	
    //             3,	 7,	11,	13,	13,	11,	 7,	 3,	
    //             2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
    //            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,				
    //        };

    //        retval.PcSqBishopStart = new int[] { 
    //            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
    //            -3,	 9,	11,	11,	11,	11,	 9,	-3,	
    //            -3,	 8,	12,	12,	12,	12,	 8,	-3,	
    //            -3,	 0,	12,	12,	12,	12,	 6,	-3,	
    //            -3,	 0,	12,	12,	12,	12,	 6,	-3,	
    //             6,	 8,	12,	12,	12,	12,	 8,	 6,	
    //             6,	 9,	11,	11,	11,	11,	 9,	 6,	
    //            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,			
    //        };


    //        retval.PcSqBishopEnd = new int[] { 
    //            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
    //            -3,	 9,	11,	11,	11,	11,	 9,	-3,	
    //            -3,	 8,	12,	12,	12,	12,	 8,	-3,	
    //            -3,	 0,	12,	12,	12,	12,	 6,	-3,	
    //            -3,	 0,	12,	12,	12,	12,	 6,	-3,	
    //             6,	 8,	12,	12,	12,	12,	 8,	 6,	
    //             6,	 9,	11,	11,	11,	11,	 9,	 6,	
    //            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,					
    //        };

    //        retval.PcSqRookStart = new int[] { 
    //            0,	0,	4,	6,	6,	4,	0,	0,	
    //            10,	10,	14,	16,	16,	14,	10,	10,
    //            0,	0,	4,	6,	6,	4,	0,	0,
    //            0,	0,	4,	6,	6,	4,	0,	0,
    //            0,	0,	4,	6,	6,	4,	0,	0,	
    //            0,	0,	4,	6,	6,	4,	0,	0,	
    //            0,	0,	4,	6,	6,	4,	0,	0,	
    //            0,	0,	4,	6,	6,	4,	0,	0			
    //        };

    //        retval.PcSqRookEnd = new int[] { 
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0,
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0			
    //        };

    //        retval.PcSqQueenStart = new int[] { 
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,  0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	2,	2,	0,	0,	0,	
    //            0,	0,	0,	2,	2,	0,	0,	0,	
    //            0,	1,	1,	1,	1,	0,	0,	0,	
    //            0,	0,	1,	1,	1,	0,	0,	0,	
    //            -2,-2, -2,  0,	0, -2, -2, -2
    //        };

    //        retval.PcSqQueenEnd = new int[] { 
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,  0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	0,	0,	0,	0,	0,	
    //            0,	0,	0,	2,	2,	0,	0,	0,	
    //            0,	0,	0,	2,	2,	0,	0,	0,	
    //            0,	1,	1,	1,	1,	0,	0,	0,	
    //            0,	0,	1,	1,	1,	0,	0,	0,	
    //            -2,-2, -2,  0,	0, -2, -2, -2		
    //        };

    //        retval.PcSqKingStart = new int[] { 
    //            -80,-80,-80,-80,-80,-80,-80,-80,
    //            -80,-80,-80,-80,-80,-80,-80,-80,
    //            -80,-80,-80,-80,-80,-80,-80,-80,
    //            -60,-60,-60,-60,-60,-60,-60,-60,
    //            -40,-40,-40,-40,-40,-40,-40,-40,
    //            -7,	-15,-15,-15,-15,-15,-15,-7,	
    //            -5,	-5,	-12,-12,-12,-12,-5,	-5,	
    //             3,	 3,	 8,	-5,  -8,-5, 10,	 5		
    //        };

    //        retval.PcSqKingEnd = new int[] { 
    //             -8, -8, -8, -8, -8, -8, -8, -8,
    //             -8, -0, -0, -0, -0, -0, -0, -8,
    //             -8, -0, 05, 05, 05, 05, -0, -8,
    //             -8, -0, 05, 30, 30, 05, -0, -8,
    //             -8, -0, 05, 30, 30, 05, -0, -8,
    //             -8, -0, 05, 30, 30, 05, -0, -8,
    //             -8, -0, -0, -0, -0, -0, -0, -8,
    //             -8, -8, -8, -8, -8, -8, -8, -8,		
    //        };

    //        retval.PawnDoubledStart = 10;
    //        retval.PawnDoubledEnd = 25;
    //        retval.PawnIsoStart = 15;
    //        retval.PawnIsoEnd = 20;

    //        retval.PawnPassedStart = new int[] { 
    //              0,  0,  0,  0,  0,  0,  0,  0,
    //             50, 50, 50, 50, 50, 50, 50, 50,
    //             30, 30, 30, 30, 30, 30, 30, 30,
    //             20, 20, 20, 20, 20, 20, 20, 20,
    //             15, 15, 15, 15, 15, 15, 15, 15,
    //             15, 15, 15, 15, 15, 15, 15, 15,
    //             15, 15, 15, 15, 15, 15, 15, 15,
    //              0,  0,  0,  0,  0,  0,  0,  0
    //        };
    //        retval.PawnPassedEnd = new int[] { 
    //              0,  0,  0,  0,  0,  0,  0,  0,
    //            140,140,140,140,140,140,140,140,
    //             90, 90, 90, 90, 90, 90, 90, 90,
    //             60, 60, 60, 60, 60, 60, 60, 60,
    //             40, 40, 40, 40, 40, 40, 40, 40,
    //             25, 25, 25, 25, 25, 25, 25, 25,
    //             15, 15, 15, 15, 15, 15, 15, 15,
    //              0,  0,  0,  0,  0,  0,  0,  0	
    //        };

    //        return retval;
    //    }

        

    //}
}
