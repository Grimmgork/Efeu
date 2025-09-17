using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    public interface IWorkflowRuntimeExportSerializer
    {
        public byte[] Serialize(WorkflowRuntimeExport export);

        public WorkflowRuntimeExport Deserialize(byte[] data);
    }
}
