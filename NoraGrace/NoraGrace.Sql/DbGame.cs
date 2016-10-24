using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NoraGrace.Sql
{
    public class DbGame
    {
        public DbGame()
        {
            Moves = new List<DbMove>();
        }

        [Key]
        public int GameId { get; set; }

        public string White { get; set; }
        public string Black { get; set; }
        
        public List<DbMove> Moves { get; set; }

        public NoraGrace.Engine.GameResult? Result { get; set; }

        public NoraGrace.Engine.GameResultReason? ResultReason { get; set; }
    }
}
