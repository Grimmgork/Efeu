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

        public bool IsCompleted => isCompleted;

        public EfeuValue Output;

        public string Fault = "";


        private bool isCompleted;

        private readonly Func<EffectExecutionContext, Task> completeAsync;

        private readonly Func<EffectExecutionContext, Task> faultAsync;

        public EffectExecutionContext(int id, Guid corellationId, DateTimeOffset timestamp, uint times, EfeuValue input, Func<EffectExecutionContext, Task> completeAsync, Func<EffectExecutionContext, Task> faultAsync)
        {
            Id = id;
            CorellationId = corellationId;
            Times = times;
            Input = input;
            Timestamp = timestamp;
            this.completeAsync = completeAsync;
            this.faultAsync = faultAsync;
        }

        public Task CompleteAsync() => completeAsync(this).ContinueWith((task) => { 
            isCompleted = true;
            return Task.CompletedTask;
        });

        public Task FaultAsync() => faultAsync(this);
    }
}
