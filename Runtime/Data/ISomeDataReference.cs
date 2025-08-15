using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public interface ISomeDataReference
    {
        public int ToInt32();
        public string ToString();
        public long ToInt64();
        public Guid ToGuid();
        public string ToName();
    }
}
