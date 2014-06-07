﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using Sinobyl.Engine.Evaluation;

namespace Sinobyl.EvalTune
{
    public interface IEvalSettingsMutator
    {
        void Mutate(ChessEvalSettings settings);
        IEnumerable<IEvalSettingsMutator> SimilarMutators();
    }
}
