using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuString : EfeuObject
    {
        private readonly string Text;

        public EfeuString(string value)
        {
            this.Text = value;
        }

        public override string ToString()
        {
            return Text;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
}
