using Efeu.Router.Data;
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

        public EffectExecutionContext(int id, Guid corellationId, uint times, EfeuValue input)
        {
            Id = id;
            CorellationId = corellationId;
            Times = times;
            Input = input;
        }
    }
}
