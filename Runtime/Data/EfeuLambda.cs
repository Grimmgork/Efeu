using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class EfeuLambda : EfeuObject
    {
        public readonly Func<EfeuValue, EfeuValue[]> Lambda;

        public EfeuLambda(Func<EfeuValue, EfeuValue[]> func)
        {
            this.Lambda = func;
        }
    }
}
