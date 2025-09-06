using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class WorkflowRouteEntity
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public int To { get; set; }
        public uint RunningNumber { get; set; }
    }
}
