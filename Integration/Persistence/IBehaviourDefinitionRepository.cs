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
        public Task<BehaviourDefinitionEntity> GetNewestByNameAsync(string name);

        public Task<BehaviourDefinitionEntity> GetByIdAsync(int id);

        public Task<BehaviourDefinitionEntity[]> GetByIdsAsync(int[] ids);

        public Task<BehaviourDefinitionEntity[]> GetAllAsync();

        public Task<int> CreateAsync(BehaviourDefinitionEntity definition);

        public Task DeleteAsync(int id);
    }
}
