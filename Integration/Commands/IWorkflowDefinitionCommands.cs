using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IWorkflowDefinitionCommands
    {
        public Task Create(string name);

        public Task Delete(int id);
    }
}
