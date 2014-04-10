using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public static class MagicBitboards
    {

        #region magicarrays

        private static readonly ulong[] _bishopMagics = new ulong[]{
            11533720853379293696UL, //A8
            4634221887939944448UL, //B8
            2469107311143059968UL, //C8
            4631963212403773698UL, //D8
            4612816453819826288UL, //E8
            286010531514368UL, //F8
            9522318258233933892UL, //G8
            44564589584384UL, //H8
            4611844485559689760UL, //A7
            9234639900802844704UL, //B7
            1152939099510816896UL, //C7
            4612253436254175747UL, //D7
            163611745976713216UL, //E7
            9079773542227200UL, //F7
            579279902869555712UL, //G7
            276019808288UL, //H7
            2533292243167252UL, //A6
            578818242689180172UL, //B6
            4521196125167876UL, //C6
            4613940019445383360UL, //D6
            9288709128585248UL, //E6
            9223408325043949572UL, //F6
            288511859852706384UL, //G6
            2306406097981834240UL, //H6
            9242019754598990849UL, //A5
            2319397788830531718UL, //B5
            2306970008900665377UL, //C5
            6917599675625308672UL, //D5
            281543712981000UL, //E5
            9799974643371622914UL, //F5
            864977001780478404UL, //G5
            576779610692288648UL, //H5
            5766863789614178434UL, //A4
            2342507332688152577UL, //B4
            4629772993306838017UL, //C4
            11261200239689856UL, //D4
            2341946590220009728UL, //E4
            2378041967813656835UL, //F4
            9259966266318524544UL, //G4
            2315133883006616576UL, //H4
            11260116309381120UL, //A3
            81137431994175636UL, //B3
            9223935056737747972UL, //C3
            36169809657856512UL, //D3
            36037597411148800UL, //E3
            9223671246053441600UL, //F3
            4556930643132672UL, //G3
            288516266363846720UL, //H3
            36877689021792364UL, //A2
            1848165255562330128UL, //B2
            75333735286019UL, //C2
            182431004186124418UL, //D2
            324681420266274946UL, //E2
            583849532784640UL, //F2
            4538788311334912UL, //G2
            2260630406449152UL, //H2
            18085883964031488UL, //A1
            709176350153515040UL, //B1
            72061992638613568UL, //C1
            1152921513742304320UL, //D1
            4611686091979247680UL, //E1
            9259409771768840320UL, //F1
            3396400276832386UL, //G1
            866978129825105952UL, //H1
        };


        private static readonly ulong[] _rookMagics = new ulong[]{
            162134001962717200UL, //A8
            4773833494088534050UL, //B8
            19141433397088264UL, //C8
            9223521639172670016UL, //D8
            1155173858538439168UL, //E8
            2310383994388153344UL, //F8
            4612249518690288656UL, //G8
            1126486208159744UL, //H8
            563019008450560UL, //A7
            9223513325172695040UL, //B7
            288371251080069250UL, //C7
            141287378391040UL, //D7
            4936085963446419584UL, //E7
            2594214672676684288UL, //F7
            2306124492780404740UL, //G7
            146402448079333409UL, //H7
            10666777933697269760UL, //A6
            581114435804872704UL, //B6
            9232379512062216192UL, //C6
            612630836834994176UL, //D6
            38280871777681920UL, //E6
            141287311278592UL, //F6
            9232520523387240704UL, //G6
            144117391666872340UL, //H6
            1443553249050231300UL, //A5
            4647750000893231168UL, //B5
            288247970487337092UL, //C5
            17594334052482UL, //D5
            36873294964064262UL, //E5
            1155175505591533696UL, //F5
            2252901481185792UL, //G5
            2300186952991744UL, //H5
            74379917248233472UL, //A4
            9547666396553428996UL, //B4
            17594341924868UL, //C4
            9232388034358415360UL, //D4
            90076392749795330UL, //E4
            49543996103459328UL, //F4
            9367627971017310464UL, //G4
            2449978126074118692UL, //H4
            4611703610647117828UL, //A3
            252236832228524032UL, //B3
            450377555459997824UL, //C3
            4611705810177884168UL, //D3
            2395923798139404292UL, //E3
            563018807246853UL, //F3
            4755802306048458880UL, //G3
            1730510915260391433UL, //H3
            9804354118411158018UL, //A2
            4035260451569827968UL, //B2
            12700168543520424064UL, //C2
            18155170358624384UL, //D2
            9277424028543385728UL, //E2
            2253998904082560UL, //F2
            2594354877555540224UL, //G2
            145247135296653344UL, //H2
            45106365017888768UL, //A1
            1155213027505770496UL, //B1
            2310348812295603206UL, //C1
            2308095910959252480UL, //D1
            4612812472419090496UL, //E1
            288815320641577024UL, //F1
            149603376760320UL, //G1
            70378005201920UL, //H1
        };

        #endregion

        public static ChessBitboard BishopAttacks(ChessPosition pos, ChessBitboard allPieces)
        {
            //mask all pieces to relevant squares
            //multiply by magic number (index now stored in high order bits)
            //shift result to lower bits
            //lookup attacks in table
            return _bishopLookup[(int)pos][((ulong)(allPieces & _bishopMask[(int)pos]) * _bishopMagics[(int)pos]) >> _bishopShift[(int)pos]];
        }

        public static ChessBitboard RookAttacks(ChessPosition pos, ChessBitboard allPieces)
        {
            //mask all pieces to relevant squares
            //multiply by magic number (index now stored in high order bits)
            //shift result to lower bits
            //lookup attacks in table
            return _rookLookup[(int)pos][((ulong)(allPieces & _rookMask[(int)pos]) * _rookMagics[(int)pos]) >> _rookShift[(int)pos]];
        }

        public static void Test()
        {
            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = BishopMask(position);

                foreach (var bitset in Combinations(mask))
                {
                    var calcAttacks = BishopAttacksCalc(position, bitset);
                    var lookupAttacks = BishopAttacks(position, bitset);

                    if (calcAttacks != lookupAttacks)
                    {
                        throw new Exception("bad magic");
                    }
                }
            }

            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = RookMask(position);

                foreach (var bitset in Combinations(mask))
                {
                    var calcAttacks = RookAttacksCalc(position, bitset);
                    var lookupAttacks = RookAttacks(position, bitset);

                    if (calcAttacks != lookupAttacks)
                    {
                        throw new Exception("bad magic");
                    }
                }
            }

            Console.WriteLine("all good");

        }

        private static ChessBitboard[][] _bishopLookup = new ChessBitboard[64][];
        private static ChessBitboard[][] _rookLookup = new ChessBitboard[64][];
        private static int[] _bishopShift = new int[64];
        private static int[] _rookShift = new int[64];
        private static ChessBitboard[] _bishopMask = new ChessBitboard[64];
        private static ChessBitboard[] _rookMask = new ChessBitboard[64];

        static MagicBitboards()
        {

            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = BishopMask(position);
                var possibleCombinations = 1 << mask.BitCount();
                var shift = 64 - mask.BitCount();

                _bishopShift[(int)position] = shift;
                _bishopMask[(int)position] = mask;

                var magic = _bishopMagics[(int)position];

                _bishopLookup[(int)position] = new ChessBitboard[possibleCombinations];

                foreach (var bitset in Combinations(mask))
                {
                    var attacks = BishopAttacksCalc(position, bitset);
                    var index = (magic * (ulong)bitset) >> shift;
                    _bishopLookup[(int)position][index] = attacks;
                }
            }

            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = RookMask(position);
                var possibleCombinations = 1 << mask.BitCount();
                var shift = 64 - mask.BitCount();

                _rookShift[(int)position] = shift;
                _rookMask[(int)position] = mask;

                var magic = _rookMagics[(int)position];

                _rookLookup[(int)position] = new ChessBitboard[possibleCombinations];

                foreach (var bitset in Combinations(mask))
                {
                    var attacks = RookAttacksCalc(position, bitset);
                    var index = (magic * (ulong)bitset) >> shift;
                    _rookLookup[(int)position][index] = attacks;
                }
            }
        }

        private static IEnumerable<ChessBitboard> Combinations(ChessBitboard mask)
        {
            var positions = mask.ToPositions().ToArray();
            var bitCount = mask.BitCount();
            var possibleCombinations = 1 << mask.BitCount();
            for (int i = 0; i < possibleCombinations; i++)
            {
                ChessBitboard combo = ChessBitboard.Empty;
                for (int b = 0; b < bitCount; b++)
                {
                    int r = i & (1 << b);
                    if (r != 0)
                    {
                        combo |= positions[b].Bitboard();
                    }
                }
                yield return combo;
            }

        }

        private const ChessBitboard EDGES = ChessBitboard.Rank1 | ChessBitboard.Rank8 | ChessBitboard.FileA | ChessBitboard.FileH;



        static ChessBitboard RookMask(ChessPosition position)
        {
            ChessBitboard attacks = RookAttacksCalc(position, ChessBitboard.Empty);
            ChessBitboard retval = attacks & ~EDGES;
            return retval;
        }

        static ChessBitboard BishopMask(ChessPosition position)
        {
            ChessBitboard attacks = BishopAttacksCalc(position, ChessBitboard.Empty);
            ChessBitboard retval = attacks & ~EDGES;
            return retval;
        }

        static ChessBitboard RookAttacksCalc(ChessPosition position, ChessBitboard blockers)
        {
            ChessBitboard retval = ChessBitboard.Empty;
            foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsRook)
            {
                ChessPosition p = position.PositionInDirection(dir);
                while (p != ChessPosition.OUTOFBOUNDS)
                {
                    retval |= p.Bitboard();
                    if (blockers.Contains(p)) { break; }
                    p = p.PositionInDirection(dir);
                }
            }
            return retval;
        }

        static ChessBitboard BishopAttacksCalc(ChessPosition position, ChessBitboard blockers)
        {
            ChessBitboard retval = ChessBitboard.Empty;
            foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsBishop)
            {
                ChessPosition p = position.PositionInDirection(dir);
                while (p != ChessPosition.OUTOFBOUNDS)
                {
                    retval |= p.Bitboard();
                    if (blockers.Contains(p)) { break; }
                    p = p.PositionInDirection(dir);
                }
            }
            return retval;
        }



        public static class Generation
        {
            private static readonly Random _rand = new Random(23456);
            private static ulong Rand64()
            {
                byte[] bytes = new byte[8];
                _rand.NextBytes(bytes);
                ulong retval = 0;
                for (int i = 0; i <= 7; i++)
                {
                    //ulong ibyte = (ulong)bytes[i]&256;
                    ulong ibyte = (ulong)bytes[i];
                    retval |= ibyte << (i * 8);
                }
                return retval;
            }

            static ulong Rand64Few()
            {
                return Rand64() & Rand64() & Rand64();
            }

            static ChessBitboard index_to_mask(int index, int bitCount, ChessBitboard m)
            {
                int i;
                ulong result = 0UL;
                for (i = 0; i < bitCount; i++)
                {

                    var j = m.NorthMostPosition();
                    m = m & ~j.Bitboard();

                    if ((index & (1 << i)) != 0)
                    {
                        result |= (1UL << (int)j);
                    }
                }
                return (ChessBitboard)result;
            }


            private static int Transform(ulong b, ulong magic, int bitCount)
            {
                return (int)((b * magic) >> (64 - bitCount));
            }
            public static int totalCalcs = 0;

            private static ulong find_magic(ChessPosition sq, bool bishop)
            {
                ChessBitboard[] b = new ChessBitboard[4096];
                ChessBitboard[] a = new ChessBitboard[4096];
                ChessBitboard[] used = new ChessBitboard[4096];

                ChessBitboard allAttacks = bishop ? BishopAttacksCalc(sq, ChessBitboard.Empty) : RookAttacksCalc(sq, ChessBitboard.Empty);
                ChessBitboard mask = allAttacks & ~EDGES;

                int j, k;

                int bitCount = mask.BitCount();
                int possibleCombinations = 1 << bitCount;
                ulong bitsLow = (1UL << bitCount) - 1UL;
                ulong bitsHigh = bitsLow << (64 - bitCount);

                for (int i = 0; i < possibleCombinations; i++)
                {
                    b[i] = index_to_mask(i, bitCount, mask);
                    a[i] = bishop ? BishopAttacksCalc(sq, b[i]) : RookAttacksCalc(sq, b[i]);
                }

                for (k = 0; k < 10000000; k++)
                {
                    totalCalcs++;

                    ulong magic = Rand64Few();

                    var mul = (ulong)mask * magic;

                    var mulmask = mul & bitsHigh;
                    if (mulmask != bitsHigh) { continue; }


                    for (int i = 0; i < 4096; i++) { used[i] = 0UL; }

                    bool fail = false;
                    for (int i = 0; i < possibleCombinations; i++)
                    {
                        j = Transform((ulong)b[i], magic, bitCount);
                        if (used[j] == 0UL)
                        {
                            used[j] = a[i];
                        }
                        else if (used[j] != a[i])
                        {
                            fail = true;
                            break;
                        }
                    }
                    if (!fail)
                    {
                        return magic;
                    }
                }

                return 0UL;
            }

            public static void FindMagics()
            {
                try
                {
                    //find bishops
                    Console.WriteLine("private static readonly ulong[] _bishopMagics = new ulong[]{");
                    foreach (ChessPosition p in ChessPositionInfo.AllPositions)
                    {
                        //var mask = BishopMask(p);
                        //var maskCount = mask.BitCount();
                        var magic = find_magic(p, true);
                        Console.WriteLine(string.Format("\t{0}UL, //{1}", magic, p));
                    }
                    Console.WriteLine("};");


                    //find rooks
                    Console.WriteLine("\n\nprivate static readonly ulong[] _rookMagics = new ulong[]{");
                    foreach (ChessPosition p in ChessPositionInfo.AllPositions)
                    {
                        //var mask = RookMask(p);
                        //var maskCount = mask.BitCount();
                        var magic = find_magic(p, false);
                        Console.WriteLine(string.Format("\t{0}UL, //{1}", magic, p));
                    }
                    Console.WriteLine("};");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }


        }
    }
}
