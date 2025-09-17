using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class EfeuFloat : EfeuObject
    {
        public readonly double Value;

        public EfeuFloat(double value)
        {
            this.Value = value;
        }

        public override bool ToBoolean()
        {
            return Value == 0;
        }

        public override double ToDouble()
        {
            return Value;
        }

        public override decimal ToDecimal()
        {
            return (decimal)Value;
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
