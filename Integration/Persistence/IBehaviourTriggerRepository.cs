using Efeu.Integration.Entities;
using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourTriggerRepository
    {
        public Task<BehaviourTriggerEntity> GetByIdAsync(Guid id);

        public Task<BehaviourTriggerEntity[]> GetAllAsync();

        public Task<BehaviourTriggerEntity[]> GetMatchingAsync(string name, EfeuMessageTag tag, Guid triggerId, DateTimeOffset timestamp);

        public Task<int> CreateAsync(BehaviourTriggerEntity trigger);

        public Task CreateBulkAsync(BehaviourTriggerEntity[] triggers);

        public Task<BehaviourTriggerEntity[]> GetStaticAsync(int definitionVersionId);

        public Task DeleteStaticAsync(int DefinitionVersionId);

        public Task DeleteAsync(Guid id);

        public Task DeleteBulkAsync(Guid[] ids);
    }
}
