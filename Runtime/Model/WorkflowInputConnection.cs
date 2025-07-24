using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public class WorkflowInputConnection
    {
        public int FromId { get; set; }

        public string FromName { get; set; }

        public int ToId { get; set; }

        public string ToName { get; set; }
    }
}
