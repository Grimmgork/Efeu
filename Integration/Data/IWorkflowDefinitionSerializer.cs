using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    public interface IWorkflowDefinitionSerializer
    {
        public WorkflowDefinition Deserialize(string payload);

        public string Serialize(WorkflowDefinition definition);
    }
}
