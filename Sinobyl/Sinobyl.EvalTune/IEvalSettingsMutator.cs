using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

namespace Sinobyl.EvalTune
{
    public interface IEvalSettingsMutator
    {
        void Mutate(ChessEvalSettings settings);
        IEnumerable<IEvalSettingsMutator> SimilarMutators();
    }
}
