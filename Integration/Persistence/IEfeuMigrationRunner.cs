using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IEfeuMigrationRunner
    {
        public int[] GetAllVersions();

        public Task MigrateToAsync(int version = 0);

        public Task<IEfeuMigration> GetMigrationInstance(int version);

        public Task<int> GetAppliedVersion();

        public Task Setup();
    }
}
