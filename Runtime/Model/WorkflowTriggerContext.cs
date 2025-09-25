using Efeu.Runtime.Data;
using Efeu.Runtime.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public class WorkflowTriggerContext
    {
        public readonly EfeuValue Input;

        public EfeuValue Data;

        public EfeuValue Output;

        public WorkflowTriggerHash Hash;
    }
}
