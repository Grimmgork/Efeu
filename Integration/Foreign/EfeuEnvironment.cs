using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public class EfeuEnvironment
    {
        public readonly IEffectProvider EffectProvider;

        public EfeuEnvironment(IEffectProvider effectProvider)
        {
            this.EffectProvider = effectProvider;
        }
    }
}
