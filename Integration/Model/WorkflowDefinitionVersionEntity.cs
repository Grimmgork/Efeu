using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Model
{
    public class WorkflowDefinitionVersionEntity
    {
        public int Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset Created { get; set; }

        public int WorkflowDefinitionId { get; set; }
    }
}
