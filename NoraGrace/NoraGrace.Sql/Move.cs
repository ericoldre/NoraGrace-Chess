
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoraGrace.Sql
{
    public class Move
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GameId { get; set; }
        public Game Game { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ply { get; set; }

        public NoraGrace.Engine.Position From { get; set; }
        public NoraGrace.Engine.Position To { get; set; }
        public NoraGrace.Engine.Piece? Promote { get; set; }

    }
}
