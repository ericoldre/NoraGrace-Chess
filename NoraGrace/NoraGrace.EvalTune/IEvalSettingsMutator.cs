using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;

namespace NoraGrace.EvalTune
{
    public interface IEvalSettingsMutator
    {
        void Mutate(Settings settings);
        IEnumerable<IEvalSettingsMutator> SimilarMutators();
    }
}
