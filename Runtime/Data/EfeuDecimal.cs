using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class EfeuDecimal : EfeuObject
    {
        public readonly decimal Value;

        public EfeuDecimal(decimal value)
        {
            Value = value;
        }

        public override double ToDouble()
        {
            return (double)Value;
        }

        public override bool ToBoolean()
        {
            return Value == 0;
        }

        public override decimal ToDecimal()
        {
            return Value;
        }

        public override long ToLong()
        {
            return (long)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
