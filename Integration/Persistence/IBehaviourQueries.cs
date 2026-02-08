using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourQueries
    {
        public Task<BehaviourEntity?> GetByIdAsync(int id);

        public Task<BehaviourVersionEntity[]> GetVersionsByIdsAsync(int[] ids);

        public Task<BehaviourVersionEntity?> GetVersionByIdAsync(int id);

        public Task<BehaviourEntity[]> GetAllAsync();

        public Task<int> CreateAsync(BehaviourEntity entity);

        public Task<int> CreateVersionAsync(BehaviourVersionEntity behaviourVersion);

        public Task UpdateAsync(BehaviourEntity entity);

        public Task<BehaviourVersionEntity?> GetLatestVersionAsync(int behaviourId);

        public Task DeleteAsync(int id);
    }
}
