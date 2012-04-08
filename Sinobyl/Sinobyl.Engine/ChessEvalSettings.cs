﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{

    public class ChessEvalSettings
    {

        #region DefaultEvalSettings

        public static ChessEvalSettings Default()
        {
            ChessEvalSettings retval = new ChessEvalSettings()
            {
                MaterialValues = new ChessPieceTypeDictionary<ChessGameStageDictionary<int>>()
                { 
                    Pawn = new ChessGameStageDictionary<int>() { Opening = 100, Endgame = 100 },
                    Knight = new ChessGameStageDictionary<int>() { Opening = 300, Endgame = 300 },
                    Bishop = new ChessGameStageDictionary<int>() { Opening = 300, Endgame = 300 },
                    Rook = new ChessGameStageDictionary<int>() { Opening = 500, Endgame = 500 },
                    Queen = new ChessGameStageDictionary<int>() { Opening = 900, Endgame = 900 },
                    King = new ChessGameStageDictionary<int>() { Opening = 0, Endgame = 0 },
                },

                MaterialBishopPair = new ChessGameStageDictionary<int>() { Opening = 30, Endgame = 50 },

                PcSqTables = new ChessPieceTypeDictionary<ChessGameStageDictionary<ChessPositionDictionary<int>>>()
                {
                    Pawn = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            0,  0,  0,  0,  0,  0,  0,  0,	
			                -10, -5, 0, 5,  5,  0, -5, -10,
			                -10, -5, 0, 5,  5,  0, -5, -10,	
			                -10, -5, 0, 10, 10, 0, -5, -10,	
			                -10, -5, 5, 15, 15, 0, -5, -10,	
			                -10, -5, 5, 10, 10, 0, -5, -10,	
			                -10, -5, 0, 5,  5,  5, -5, -10,	
			                 0,  0,  0,  0,  0,  0,  0,  0,	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            0,  0,  0,  0,  0,  0,  0,  0,	
			                20, 25, 34, 38, 38, 34, 25, 20,
			                12, 20, 24, 32, 32, 24, 20, 12,	
			                 6, 11, 18,	27, 27, 16,	11,  6,	
			                 4,  7, 10, 12, 12, 10,  7,  4,	
			                -3, -3, -3, -3, -3, -3, -3,  -3,	
			                -10,-10,-10,-10,-10,-10,-10,-10,	
			                 0,  0,  0,  0,  0,  0,  0,  0,	
                        })
                    },
                    Knight = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			                 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			                 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			                 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			                 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			                 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			                 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        })
                    },
                    Bishop = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			                -3,	 9,	11,	11,	11,	11,	 9,	-3,	
			                -3,	 8,	12,	12,	12,	12,	 8,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			                 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			                -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			                -3,	 9,	11,	11,	11,	11,	 9,	-3,	
			                -3,	 8,	12,	12,	12,	12,	 8,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			                 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			                -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,		
                        })
                    },
                    Rook = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            0,	0,	4,	6,	6,	4,	0,	0,	
			                10,	10,	14,	16,	16,	14,	10,	10,
			                0,	0,	4,	6,	6,	4,	0,	0,
			                0,	0,	4,	6,	6,	4,	0,	0,
			                0,	0,	4,	6,	6,	4,	0,	0,	
			                0,	0,	4,	6,	6,	4,	0,	0,	
			                0,	0,	4,	6,	6,	4,	0,	0,	
			                0,	0,	4,	6,	6,	4,	0,	0	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0	
                        })
                    },
                    Queen = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                      		0,	0,	0,	0,	0,	0,	0,	0,	
			                0,  0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	1,	1,	1,	1,	0,	0,	0,	
			                0,	0,	1,	1,	1,	0,	0,	0,	
			                -2,-2, -2,  0,	0, -2, -2, -2
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            0,	0,	0,	0,	0,	0,	0,	0,	
			                0,  0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	1,	1,	1,	1,	0,	0,	0,	
			                0,	0,	1,	1,	1,	0,	0,	0,	
			                -2,-2, -2,  0,	0, -2, -2, -2	
                        })
                    },
                    King = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            -80,-80,-80,-80,-80,-80,-80,-80,
			                -80,-80,-80,-80,-80,-80,-80,-80,
			                -80,-80,-80,-80,-80,-80,-80,-80,
			                -60,-60,-60,-60,-60,-60,-60,-60,
			                -40,-40,-40,-40,-40,-40,-40,-40,
			                -7,	-15,-15,-15,-15,-15,-15,-7,	
			                -5,	-5,	-12,-12,-12,-12,-5,	-5,	
			                 3,	 3,	 8,	-5,  -8,-5, 10,	 5		
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                             -8, -8, -8, -8, -8, -8, -8, -8,
			                 -8, -0, -0, -0, -0, -0, -0, -8,
			                 -8, -0, 05, 05, 05, 05, -0, -8,
			                 -8, -0, 05, 30, 30, 05, -0, -8,
			                 -8, -0, 05, 30, 30, 05, -0, -8,
			                 -8, -0, 05, 30, 30, 05, -0, -8,
			                 -8, -0, -0, -0, -0, -0, -0, -8,
			                 -8, -8, -8, -8, -8, -8, -8, -8,
                        })
                    },
                },


                
                PawnPassedValues = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                {
                    Opening = new ChessPositionDictionary<int>(new int[]
                     {
                          0,  0,  0,  0,  0,  0,  0,  0,
			             50, 50, 50, 50, 50, 50, 50, 50,
			             30, 30, 30, 30, 30, 30, 30, 30,
			             20, 20, 20, 20, 20, 20, 20, 20,
			             15, 15, 15, 15, 15, 15, 15, 15,
			             15, 15, 15, 15, 15, 15, 15, 15,
			             15, 15, 15, 15, 15, 15, 15, 15,
			              0,  0,  0,  0,  0,  0,  0,  0
                     }),
                    Endgame = new ChessPositionDictionary<int>(new int[]
                     {
                      	  0,  0,  0,  0,  0,  0,  0,  0,
			            140,140,140,140,140,140,140,140,
			             90, 90, 90, 90, 90, 90, 90, 90,
			             60, 60, 60, 60, 60, 60, 60, 60,
			             40, 40, 40, 40, 40, 40, 40, 40,
			             25, 25, 25, 25, 25, 25, 25, 25,
			             15, 15, 15, 15, 15, 15, 15, 15,
			              0,  0,  0,  0,  0,  0,  0,  0	
                     })
                },
                PawnDoubled = new ChessGameStageDictionary<int>() { Opening = 10, Endgame = 25 },
                PawnIsolated = new ChessGameStageDictionary<int>() { Opening = 15, Endgame = 25 },

            };

            
            return retval;
        }

        #endregion


        public ChessPieceTypeDictionary<ChessGameStageDictionary<ChessPositionDictionary<int>>> PcSqTables = new ChessPieceTypeDictionary<ChessGameStageDictionary<ChessPositionDictionary<int>>>();
        public ChessPieceTypeDictionary<ChessGameStageDictionary<int>> MaterialValues = new ChessPieceTypeDictionary<ChessGameStageDictionary<int>>();
        public ChessGameStageDictionary<int> MaterialBishopPair = new ChessGameStageDictionary<int>();

        public ChessGameStageDictionary<ChessPositionDictionary<int>> PawnPassedValues = new ChessGameStageDictionary<ChessPositionDictionary<int>>();
        public ChessGameStageDictionary<int> PawnDoubled = new ChessGameStageDictionary<int>();
        public ChessGameStageDictionary<int> PawnIsolated = new ChessGameStageDictionary<int>();

        public ChessEvalSettings CloneDeep()
        {
            return Chess.DeserializeObject<ChessEvalSettings>(Chess.SerializeObject<ChessEvalSettings>(this));
        }

        public override bool Equals(object obj)
        {
            ChessEvalSettings other = obj as ChessEvalSettings;
            if (other == null) { return false; }

            var v1 = Chess.SerializeObject<ChessEvalSettings>(this);
            var v2 = Chess.SerializeObject<ChessEvalSettings>(other);
            return v1 == v2;
        }

    }

}