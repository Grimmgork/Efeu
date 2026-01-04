using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public interface IEfeuEffectProvider
    {
        public IEffect? TryGetEffect(string name);
    }
}
