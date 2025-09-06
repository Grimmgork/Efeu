using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class WorkflowTriggerEntity
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public DateTime NotBefore { get; set; }
        public string SignalHash { get; set; } = "";
    }
}
