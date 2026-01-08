using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class BehaviourDefinitionSessionEntity
    {
        public string Id = "";

        public string UserId = "";

        public BehaviourDefinitionStep[] Steps = [];

        public int BehaviourDefinitionId;

        public DateTimeOffset LockedUntil;
    }
}
