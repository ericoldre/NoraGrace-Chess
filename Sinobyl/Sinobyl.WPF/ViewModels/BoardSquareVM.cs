using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModels
{
    public class BoardSquareVM: ViewModelBase 
    {
        private readonly Sinobyl.Engine.ChessPosition _position;

        public string Name
        {
            get { return _position.PositionToString(); }
            
        }
        public bool IsLight
        {
            get { return _position.IsLight(); }
        }

        public BoardSquareVM(Sinobyl.Engine.ChessPosition pos)
        {
            _position = pos;
        }

        public override string  ToString()
        {
 	        return Name;
        }

        public Sinobyl.Engine.ChessPosition Position
        {
            get
            {
                return _position;
            }
        }

        public static List<BoardSquareVM> AllBoardSquares()
        {
            List<BoardSquareVM> retval = new List<BoardSquareVM>();
            foreach (var pos in Sinobyl.Engine.Chess.AllPositions)
            {
                retval.Add(new BoardSquareVM(pos));
            }
            return retval;
        }
    }
}
