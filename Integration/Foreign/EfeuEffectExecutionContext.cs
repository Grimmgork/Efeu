using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Router.Value;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public enum EfeuEffectResult
    {
        Completed,
        Suspend
    }

    public class EfeuEffectTrigger
    {
        public string Name = "";

        public EfeuTriggerMatch[] Fields = [];
    }

    public class EfeuEffectExecutionContext
    {
        public readonly Guid Id;

        public readonly Guid CorellationId;

        public readonly uint Times;

        public readonly DateTimeOffset Timestamp;

        public readonly EfeuValue Input;

        public EfeuValue Output;

        public readonly EfeuEffectTrigger Trigger = new EfeuEffectTrigger();

        public bool Suspend;

        public EfeuEffectExecutionContext(Guid id, Guid corellationId, DateTimeOffset timestamp, uint times, EfeuValue input)
        {
            Id = id;
            CorellationId = corellationId;
            Times = times;
            Input = input;
            Timestamp = timestamp;
        }
    }
}
