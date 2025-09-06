using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class WorkflowDefinitionVersionEntity
    {
        public int Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset Created { get; set; }

        public WorkflowDefinition Definition { get; set; } = new WorkflowDefinition();

        public int WorkflowDefinitionId { get; set; }
    }
}
