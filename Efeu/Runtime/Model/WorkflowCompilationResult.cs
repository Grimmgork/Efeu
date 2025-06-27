using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public class WorkflowCompilationResult
    {
        public bool IsSuccessful;
    }

    public enum WorkflowCompilationErrorType
    {
        InvalidOutputType,
        UnknownId
    }

    public class WorkflowCompilationError
    {
        public WorkflowCompilationErrorType Type;

        public int ActionId;

        public string ArgumentName = "";
    }
}
