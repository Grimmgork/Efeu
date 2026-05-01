using Efeu.Runtime.Value;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Efeu.Runtime
{
    public class EfeuRuntimeLoopback
    {
        public readonly string Position = "";

        public readonly EfeuBehaviourStep Step = new EfeuBehaviourStep();

        // public readonly EfeuArray Iterator = EfeuArray.Empty;

        public readonly EfeuRuntimeScope Scope = EfeuRuntimeScope.Empty;

        public EfeuRuntimeLoopback(string position, EfeuBehaviourStep step, EfeuRuntimeScope scope)
        {
            this.Position = position;
            this.Step = step;
            // this.Iterator = iterator;
            this.Scope = scope;
        }
    }

    public class EfeuRuntimeScope
    {
        public readonly Guid Id;

        public readonly ImmutableDictionary<string, EfeuValue> Constants;

        public readonly EfeuRuntimeLoopback? Loopback;

        public static readonly EfeuRuntimeScope Empty = new EfeuRuntimeScope(Guid.Empty, ImmutableDictionary<string, EfeuValue>.Empty);

        public EfeuRuntimeScope(Guid id, ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Id = id;
            this.Constants = constants;
        }

        public EfeuRuntimeScope(Guid id, ImmutableDictionary<string, EfeuValue> constants, EfeuRuntimeLoopback? loopback)
        {
            this.Id = id;
            this.Constants = constants;
            this.Loopback = loopback;
        }

        public EfeuValue Get(string name)
        {
            return Constants[name];
        }

        public EfeuRuntimeScope With(string name, EfeuValue value)
        {
            return new EfeuRuntimeScope(Guid.NewGuid(), Constants.SetItem(name, value), Loopback);
        }

        public EfeuRuntimeScope With(string name, Func<EfeuValue, EfeuValue> func)
        {
            return new EfeuRuntimeScope(Guid.NewGuid(), Constants.SetItem(name, func(Constants[name])), Loopback);
        }

        public EfeuRuntimeScope PushLoopback(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
        {
            EfeuArray iterator = EfeuArray.Empty;
            EfeuRuntimeLoopback loopback = new EfeuRuntimeLoopback(position, step, scope);
            return new EfeuRuntimeScope(Guid.NewGuid(), Constants.SetItem(step.ArgumentName, EfeuArray.Empty), loopback);
        }

        public EfeuRuntimeScope PushLoopbackIteration(EfeuValue value)
        {
            if (Loopback == null)
                throw new InvalidOperationException();

            EfeuRuntimeLoopback loopback = new EfeuRuntimeLoopback(Loopback.Position, Loopback.Step, Loopback.Scope);
            return new EfeuRuntimeScope(Guid.NewGuid(), Loopback.Scope.Constants.SetItem(Loopback.Step.ArgumentName, Loopback.Scope.Constants[Loopback.Step.ArgumentName].AsArray().Push(value)), loopback);
        }
    }
}
