using Efeu.Router;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public class EffectExecutionContext
    {
        public readonly int Id;

        public readonly Guid CorellationId;

        public readonly uint Times;

        public readonly EfeuValue Input;

        public EfeuValue Output;
    }
}
