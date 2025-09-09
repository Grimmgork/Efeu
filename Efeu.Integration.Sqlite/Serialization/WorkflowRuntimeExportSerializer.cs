using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite.Serialization
{
    internal class WorkflowRuntimeExportSerializer
    {
        // SOMEDATA [TYPE, VALUE|REF, REF, REF]
        // DICT [REF, BYTES[]]

        public string Serialize(WorkflowRuntimeExport export)
        {
            // recursively go through all data
            // if a reference is found store it in dictionary
            // store a reference to data
            
        }

        public WorkflowRuntimeExport Deserialize(string data)
        {
            // DESERIALIZE DICT
            // DESERIALIZE SOME DATA
            //    IF TYPE IS REF
            //       DESERIALIZE FROM DICT AS TYPE
        }
    }
}
