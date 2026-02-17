using Efeu.Integration.Entities;
using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface ITriggerQueries
    {
        public Task<TriggerEntity> GetByIdAsync(Guid id);

        public Task<TriggerEntity[]> GetAllAsync();

        public Task<TriggerEntity[]> GetMatchingAsync(string name, EfeuMessageTag tag, Guid matter, DateTimeOffset timestamp);

        public Task<int> CreateAsync(TriggerEntity trigger);

        public Task CreateBulkAsync(TriggerEntity[] triggers);

        public Task<TriggerEntity[]> GetStaticAsync(int definitionVersionId);

        public Task DeleteStaticAsync(int DefinitionVersionId);

        public Task DeleteAsync(Guid id);

        public Task DeleteBulkAsync(Guid[] ids);

        public Task DeleteByMatterBulkAsync(Guid[] matters);

        public Task DeleteByGroupBulkAsync(Guid[] groups);
    }
}
