using Efeu.Integration.Entities;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourDefinitionRepository
    {
        public Task<BehaviourDefinitionVersionEntity[]> GetVersionsByIdsAsync(int[] ids);

        public Task<BehaviourDefinitionEntity[]> GetAllAsync();

        public Task<int> CreateAsync(BehaviourDefinitionEntity entity);

        public Task<int> CreateVersionAsync(BehaviourDefinitionVersionEntity definition);

        public Task<BehaviourDefinitionVersionEntity?> GetNewestVersionAsync(int definitionId);

        public Task<BehaviourDefinitionVersionEntity> GetVersionByIdAsync(int id);

        public Task DeleteAsync(int id);
    }
}
