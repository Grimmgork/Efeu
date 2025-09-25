using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
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

        public override EfeuValue Call(string field)
        {
            Type type = Wrappee.GetType();
            return EfeuValue.Parse(type.GetProperty(field)?.GetValue(Wrappee, null));
        }

        public override EfeuValue Call(int index)
        {
            dynamic obj = Wrappee;
            return EfeuValue.Parse(obj[index]);
        }

        public override void Call(int index, EfeuValue value)
        {
            base.Call(index, value);
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
