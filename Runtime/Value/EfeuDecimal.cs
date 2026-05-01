using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuDecimal : EfeuObject
    {
        public readonly decimal Value;

        public EfeuDecimal(decimal value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool AsBoolean()
        {
            return Value > 0m;
        }

        public override decimal AsDecimal()
        {
            return Value;
        }

        public override double AsDouble()
        {
            return (double)Value;
        }

        public override long AsLong()
        {
            return (long)Value;
        }

        public override bool Equals(EfeuValue value)
        {
            return value.AsDecimal() == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
