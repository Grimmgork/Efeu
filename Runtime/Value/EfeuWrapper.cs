using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuWrapper : EfeuObject
    {
        public readonly object Wrappee;

        public EfeuWrapper(object wrappee)
        {
            if (wrappee is EfeuObject)
            {
                throw new InvalidOperationException($"Cannot wrap {nameof(EfeuObject)}.");
            }

            this.Wrappee = wrappee;
        }

        public override string ToString()
        {
            return Wrappee.ToString() ?? "";
        }
    }
}
