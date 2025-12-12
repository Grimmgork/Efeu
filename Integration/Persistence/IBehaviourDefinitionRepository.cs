using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourDefinitionRepository
    {
        public Task<BehaviourDefinitionEntity?> GetByIdAsync(int id);

        public Task<BehaviourDefinitionVersionEntity[]> GetVersionsByIdsAsync(int[] ids);

        public Task<BehaviourDefinitionVersionEntity?> GetVersionByIdAsync(int id);

        public Task<BehaviourDefinitionEntity[]> GetAllAsync();

        public Task<int> CreateAsync(BehaviourDefinitionEntity entity);

        public Task<int> CreateVersionAsync(BehaviourDefinitionVersionEntity definition);

        public Task UpdateAsync(BehaviourDefinitionEntity entity);

        public Task<BehaviourDefinitionVersionEntity?> GetLatestVersionAsync(int definitionId);

        public Task DeleteAsync(int id);
    }
}
