using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Script
{
    public class EfeuArgs
    {
        public EfeuValue this[int index]
        {
            get
            {
                return Items[index];
            }
        }

        public EfeuValue this[string field]
        {
            get
            {
                return Fields[field];
            }
        }

        public EfeuValue[] Items = [];

        public Dictionary<string, EfeuValue> Fields = [];

        public Func<EfeuValue, EfeuValue> Do = (args) => default;
    }
}
