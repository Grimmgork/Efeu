using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public enum EfeuTraversalType
    {
        Leaf,
        Array,
        Struct
    }

    public interface IEfeuTraversible
    {
        public IEfeuTraversible Call(string name);

        public IEfeuTraversible Call(int index);

        public EfeuValue Eval();
    }
}
