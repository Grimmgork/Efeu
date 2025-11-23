using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using Efeu.Router;
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

        public Task<IEnumerable<BehaviourTriggerEntity>> GetAllActiveAsync();

        public Task<BehaviourTriggerEntity[]> GetMatchingAsync(string name, EfeuMessageTag tag, Guid triggerId);

        public Task<int> CreateAsync(BehaviourTriggerEntity trigger);

        public Task CreateBulkAsync(BehaviourTriggerEntity[] triggers);

        public Task DeleteStaticAsync(int DefinitionVersionId);

        public Task DeleteAsync(Guid id);

        public Task DeleteBulkAsync(Guid[] ids);
    }
}
