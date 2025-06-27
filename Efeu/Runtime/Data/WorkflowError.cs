using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public struct WorkflowError
    {
        public readonly int Code;

        public WorkflowError(int code)
        {
            this.Code = code;
        }
    }
}
