using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class BehaviourEntity
    {
        public int Id;

        public string Name = "";

        public int Version;

        public EfeuBehaviourStep[] Steps = [];
    }
}
