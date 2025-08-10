using Efeu.Integration.Model;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IWorkflowDefinitionCommands
    {
        public Task CreateAsync(string name);

        public Task Publish(int id);

        public Task RollbackToVersion(int id, int versionId);

        public Task UpdateDefinitionAsync(int id, WorkflowDefinition definition);

        public Task RenameAsync(int id, string name);

        public Task Delete(int id);
    }
}
