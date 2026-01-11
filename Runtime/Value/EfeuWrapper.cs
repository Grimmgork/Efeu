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

        public override bool ToBoolean()
        {
            return Convert.ToBoolean(Wrappee);
        }

        public override decimal ToDecimal()
        {
            return Convert.ToDecimal(Wrappee);
        }

        public override double ToDouble()
        {
            return Convert.ToDouble(Wrappee);
        }

        public override long ToLong()
        {
            return Convert.ToInt64(Wrappee);
        }

        public override string ToString()
        {
            return Wrappee.ToString() ?? "";
        }
    }
}
