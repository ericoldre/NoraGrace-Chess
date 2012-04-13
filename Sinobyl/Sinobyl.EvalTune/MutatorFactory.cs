using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Sinobyl.EvalTune.Mutators;
namespace Sinobyl.EvalTune
{
    public static class MutatorFactory
    {

        public static IEvalSettingsMutator Create(Random rand)
        {
            var type = typeof(IEvalSettingsMutator);
            var infos = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Select(p => new { type = p, constructor = p.GetConstructor(new[] { typeof(Random) }) })
                .Where(p => p.constructor != null).ToList();

            if (infos.Count > 0)
            {
                int index = rand.Next(0, infos.Count);
                var info = infos[index];

                var mutator = (IEvalSettingsMutator)info.constructor.Invoke(new[] { rand });

                return mutator;
            }
            throw new Exception("could not find mutator");

               


   
        }

        

    }
}
