using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router.Data
{
    public class EfeuFloat : EfeuObject
    {
        private readonly double value;

        public EfeuFloat(double value)
        {
            this.value = value;
        }

        public override bool ToBoolean()
        {
            return value == 0;
        }

        public override double ToDouble()
        {
            return value;
        }

        public override decimal ToDecimal()
        {
            return (decimal)value;
        }

        public override EfeuValue Add(EfeuValue value)
        {
            return this.value + value.ToDouble();
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
