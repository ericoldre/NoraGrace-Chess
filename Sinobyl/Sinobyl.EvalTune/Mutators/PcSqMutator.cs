using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using Sinobyl.Engine.Evaluation;

namespace Sinobyl.EvalTune.Mutators
{

    public class PcSqMutator: IEvalSettingsMutator
    {

        public Bitboard Positions { get; private set; }
        public PieceType PieceType { get; private set; }
        public GameStage[] Stages { get; private set; }
        public int Amount { get; private set; }

        public PcSqMutator(Bitboard positions, PieceType pieceType, IEnumerable<GameStage> stages, int amount)
        {
            this.Positions = positions;
            this.PieceType = pieceType;
            this.Stages = stages.ToArray();
            this.Amount = amount;
        }

        public PcSqMutator(Random rand)
        {
            PieceType = PieceInfo.AllPieces[rand.Next(0, PieceInfo.AllPieces.Count())].ToPieceType();

            switch (rand.Next(0, 3))
            {
                case 0:
                    Stages = new GameStage[] { GameStage.Opening };
                    break;
                case 1:
                    Stages = new GameStage[] { GameStage.Endgame };
                    break;
                default:
                    Stages = new GameStage[] { GameStage.Opening, GameStage.Endgame };
                    break;

            }

            while(Math.Abs(Amount)<3)
            {
                Amount = rand.Next(-10, 11);
            }
            

            List<Bitboard> boards = new List<Bitboard>();
            boards.AddRange(PositionInfo.AllPositions.Select(p => p.ToBitboard()));
            boards.AddRange(RankInfo.AllRanks.Select(r => r.ToBitboard()));
            boards.AddRange(FileInfo.AllFiles.Select(f => f.ToBitboard()));
            boards.AddRange(PositionInfo.AllPositions.Select(p => bitboardExpand(p.ToBitboard())));

            this.Positions = boards[rand.Next(0, boards.Count())];
            
        }

        private static Bitboard bitboardExpand(Bitboard board)
        {
            return board.ShiftDirE() | board.ShiftDirN() | board.ShiftDirS() | board.ShiftDirW() | board.ShiftDirNE() | board.ShiftDirNW() | board.ShiftDirSE() | board.ShiftDirSW();
        }

        #region IEvalSettingsMutator Members

        void IEvalSettingsMutator.Mutate(Settings settings)
        {
            foreach (Position pos in this.Positions.ToPositions())
            {
                foreach (GameStage stage in this.Stages)
                {
                    settings.PcSqTables[this.PieceType][stage][pos] += this.Amount;
                }
            }
        }

        IEnumerable<IEvalSettingsMutator> IEvalSettingsMutator.SimilarMutators()
        {
            
            yield return new PcSqMutator(this.Positions, this.PieceType, new GameStage[] { GameStage.Opening }, this.Amount);
            yield return new PcSqMutator(this.Positions, this.PieceType, new GameStage[] { GameStage.Endgame }, this.Amount);
            yield return new PcSqMutator(this.Positions, this.PieceType, new GameStage[] { GameStage.Opening, GameStage.Endgame }, this.Amount);

            yield return new PcSqMutator(bitboardExpand(this.Positions), this.PieceType, this.Stages, this.Amount);
            yield return new PcSqMutator(bitboardExpand(this.Positions) & ~ this.Positions, this.PieceType, this.Stages, this.Amount);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("PcSq[{0}][{1}][{2}] {3}", this.PieceType.ToString(), this.Positions.ToString(), this.Stages.Count()>1?"BOTH":this.Stages[0].ToString(), this.Amount>0?"+= "+this.Amount.ToString(): "-= "+Math.Abs(this.Amount).ToString());
        }
    }
}
