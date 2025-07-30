using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public interface IEfeuMigrationRunner
    {
        public int[] GetAvailableVersions();

        public Task MigrateAsync(int version);

        public Task<IEfeuMigration> GetMigrationInstance(int version);

        public Task<int> GetAppliedVersion();

        public Task Setup();
    }
}
