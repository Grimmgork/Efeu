using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
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

        public async Task AttachAsync(string name, EfeuValue input)
        {

        }

        public void Register(string name, Func<IWorkflowTrigger> func)
        {
            factories.Add(name, func);
        }
    }
}
