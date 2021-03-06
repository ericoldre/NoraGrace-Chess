﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine;
using System.IO;

namespace NoraGrace.Engine.Tests
{
    [TestClass]
    public class PGNTests
    {
        [TestMethod]
        public void ParseShortPGNTest()
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.short.pgn"))
            {
                StreamReader reader = new StreamReader(stream);

                var games = PGN.AllGames(reader).ToList();
                Assert.AreEqual<int>(8, games.Count);
                Assert.IsTrue(Enumerable.SequenceEqual<int>(games.Select(g => g.Moves.Count), new int[] { 70, 44, 113, 63, 77, 135, 55, 82 }));

                var results = games.Select(g => g.Result.Value).ToArray();
                bool resultsEqual = Enumerable.SequenceEqual<GameResult>(results, new GameResult[] 
                { 
                    GameResult.Draw, 
                    GameResult.Draw,
                    GameResult.Draw,
                    GameResult.WhiteWins,
                    GameResult.WhiteWins,
                    GameResult.Draw,
                    GameResult.WhiteWins,
                    GameResult.BlackWins
                });
                Assert.IsTrue(resultsEqual);
            }
        }
    }
}
