using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourScopeCommands
    {
        public Task CreateBulkAsync(BehaviourScopeEntity[] entities);
    }
}
