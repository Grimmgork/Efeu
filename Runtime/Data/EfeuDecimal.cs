using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class EfeuDecimal : EfeuObject
    {
        private readonly decimal value;

        public EfeuDecimal(decimal value)
        {
            this.value = value;
        }

        public override double ToDouble()
        {
            return (double)value;
        }

        public override bool ToBoolean()
        {
            return value == 0;
        }

        public override decimal ToDecimal()
        {
            return value;
        }

        public override long ToLong()
        {
            return (long)value;
        }

        public override EfeuValue Add(EfeuValue value)
        {
            return value.ToDecimal() + value.ToDecimal();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}
