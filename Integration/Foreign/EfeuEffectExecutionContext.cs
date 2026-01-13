using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Value;
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
        Complete,
        Suspend
    }

    public class EfeuEffectTrigger
    {
        public readonly Guid Id = Guid.NewGuid();

        public string Name = "";

        public EfeuMessageTag Tag;
    }

    public class EfeuEffectTriggerContext
    {
        public readonly Guid Id;

        public readonly Guid CorellationId;

        public readonly DateTimeOffset Timestamp;

        public readonly EfeuValue Input;

        public readonly EfeuValue Data;

        public readonly EfeuMessage Message = new EfeuMessage();

        public Task RespondAsync(EfeuValue output) => Task.CompletedTask;

        public Task SuspendAsync() => Task.CompletedTask;

        public Task SuspendAsync(EfeuEffectTrigger trigger) => Task.CompletedTask;
    }

    public class EfeuEffectExecutionContext
    {
        public readonly Guid Id;

        public readonly Guid CorellationId;

        public readonly uint Times;

        public readonly DateTimeOffset Timestamp;

        public readonly EfeuValue Input;

        public EfeuValue Data;

        public EfeuEffectExecutionContext(Guid id, Guid corellationId, DateTimeOffset timestamp, uint times, EfeuValue input)
        {
            Id = id;
            CorellationId = corellationId;
            Times = times;
            Input = input;
            Timestamp = timestamp;
        }

        public Task RespondAsync(EfeuValue output) => Task.CompletedTask;

        public Task SuspendAsync(EfeuEffectTrigger trigger) => Task.CompletedTask;
    }
}
