using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuFloat : EfeuObject
    {
        public readonly double Value;

        public EfeuFloat(double value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool AsBoolean()
        {
            return Value > 0f;
        }

        public override decimal AsDecimal()
        {
            return (decimal)Value;
        }

        public override long AsLong()
        {
            return (long)Value;
        }

        public override double AsDouble()
        {
            return Value;
        }

        public override bool Equals(EfeuValue value)
        {
            return value.AsDouble() == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
