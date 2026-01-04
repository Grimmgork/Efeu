using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public class EfeuEnvironmentA
    {
        public readonly IEfeuEffectProvider EffectProvider;

        public EfeuEnvironmentA(IEfeuEffectProvider effectProvider)
        {
            this.EffectProvider = effectProvider;
        }
    }
}
