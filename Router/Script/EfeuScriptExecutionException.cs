using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router.Script
{
    public class EfeuScriptExecutionException : Exception
    {
        public readonly int Line;

        public readonly int Column;

        public EfeuScriptExecutionException(int line, int column, string message) : base(message)
        {
            Line = line;
            Column = column;
        }
    }
}
