using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IWorkflowDefinitionSessionRepository
    {
        public Task<WorkflowDefinitionSessionEntity> GetByIdAsync(int id);

        // public Task<Workflow>
    }
}
