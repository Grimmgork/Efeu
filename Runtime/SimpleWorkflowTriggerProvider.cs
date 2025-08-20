using Efeu.Integration.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public class SimpleWorkflowTriggerProvider : IWorkflowTriggerProvider
    {
        private readonly Dictionary<string, Func<IWorkflowTrigger>> factories = new Dictionary<string, Func<IWorkflowTrigger>>();

        public IWorkflowTrigger GetTrigger(string name)
        {
            return factories[name]();
        }
    }
}
