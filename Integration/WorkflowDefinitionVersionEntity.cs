using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public class WorkflowDefinitionVersionEntity
    {
        public int Id { get; set; }

        public int Version { get; set; }

        public DateTime Created { get; set; }

        public int WorkflowDefinitionId { get; set; }
    }
}
