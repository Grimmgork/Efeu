using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class WorkflowDefinitionSessionEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";

        public DateTimeOffset LastInteraction { get; set; }

        public int Start { get; set; }

        public List<WorkflowActionNode> Actions { get; set; } = [];
    }
}
