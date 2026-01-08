using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public readonly DateTimeOffset Timestamp;

        public readonly EfeuValue Input;

        public EfeuValue Output;

        public string Fault = "";

        public EffectExecutionContext(int id, Guid corellationId, DateTimeOffset timestamp, uint times, EfeuValue input)
        {
            Id = id;
            CorellationId = corellationId;
            Times = times;
            Input = input;
            Timestamp = timestamp;
        }
    }
}
