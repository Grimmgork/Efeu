using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Interfaces
{
    public interface IWorkflowDefinitionRepository
    {
        public Task GetByNameAsync(string name);

        public Task<WorkflowDefinition> GetById(int id);

        public Task Update();

        public Task GetAllAsync();
    }
}
