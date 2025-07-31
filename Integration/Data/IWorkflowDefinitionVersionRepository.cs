using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    public interface IWorkflowDefinitionVersionRepository
    {
        public Task GetByIdAsync(int id);

        public Task GetPublishedAsync(string name);

        public Task GetLatestVersion(int definitionId);

        public Task DeleteAsync(int id);

        public Task Create();
    }
}
