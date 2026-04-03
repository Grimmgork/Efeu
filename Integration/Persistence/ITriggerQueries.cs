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
        public Task<TriggerEntity?> GetByIdAsync(Guid id);

        public Task<TriggerEntity[]> GetAllAsync();

        public Task<int> CreateAsync(TriggerEntity trigger);

        public Task CreateBulkAsync(TriggerEntity[] triggers);

        public Task<TriggerEntity[]> GetStaticAsync(int definitionVersionId);

        public Task DetatchStaticAsync(int DefinitionVersionId);

        public Task DetatchAsync(Guid[] ids);

        public Task DetatchByMatterBulkAsync(Guid[] matters);

        public Task DetatchByGroupBulkAsync(Guid[] groups);

        public Task<TriggerEntity[]> GetByIdsAsync(params Guid[] ids);

        public Task<TriggerEntity[]> GetDetatchedAsync(int limit);

        public Task DeleteAsync(Guid[] ids);
    }
}
