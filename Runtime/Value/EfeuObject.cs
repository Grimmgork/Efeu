using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Efeu.Runtime.Value
{
    public abstract class EfeuObject
    {
        public string TypeName => this.GetType().Name;

        public override string ToString()
        {
            return $"<{TypeName}>";
        }
    }
}
