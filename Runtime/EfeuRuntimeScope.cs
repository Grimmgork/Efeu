using Efeu.Runtime.Value;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Efeu.Runtime;

public class EfeuRuntimeLoopback
{
    public readonly string Position;

    public readonly EfeuBehaviourStep Step;

    public readonly EfeuRuntimeScope Scope;

    public EfeuRuntimeLoopback(string position, EfeuBehaviourStep step, EfeuRuntimeScope scope)
    {
        this.Position = position;
        this.Step = step;
        this.Scope = scope;
    }
}

public class EfeuRuntimeScope
{
    public readonly ImmutableDictionary<string, EfeuValue> Constants;

    public readonly EfeuRuntimeLoopback? Loopback;

    public static readonly EfeuRuntimeScope Empty = new EfeuRuntimeScope(ImmutableDictionary<string, EfeuValue>.Empty);

    public EfeuRuntimeScope(ImmutableDictionary<string, EfeuValue> constants)
    {
        this.Constants = constants;
    }

    public EfeuRuntimeScope(ImmutableDictionary<string, EfeuValue> constants, EfeuRuntimeLoopback? loopback)
    {
        this.Constants = constants;
        this.Loopback = loopback;
    }

    public EfeuValue Get(string name)
    {
        return Constants[name];
    }

    public EfeuRuntimeScope With(string name, EfeuValue value)
    {
        return new EfeuRuntimeScope(Constants.SetItem(name, value), Loopback);
    }

    public EfeuRuntimeScope With(string name, Func<EfeuValue, EfeuValue> func)
    {
        return new EfeuRuntimeScope(Constants.SetItem(name, func(Constants[name])), Loopback);
    }

    public EfeuRuntimeScope PushLoopback(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
    {
        if (step.ArgumentName == null)
            throw new InvalidOperationException();
        
        EfeuRuntimeLoopback loopback = new EfeuRuntimeLoopback(position, step, scope);
        return new EfeuRuntimeScope(Constants.SetItem(step.ArgumentName, EfeuArray.Empty), loopback);
    }

    public EfeuRuntimeScope PushLoopbackIteration(EfeuValue value)
    {
        if (Loopback == null)
            throw new InvalidOperationException();
        
        if (Loopback.Step.ArgumentName == null)
            throw new InvalidOperationException();

        EfeuRuntimeLoopback loopback = new EfeuRuntimeLoopback(Loopback.Position, Loopback.Step, Loopback.Scope);
        EfeuValue iterator = Constants[Loopback.Step.ArgumentName];
        return new EfeuRuntimeScope(Loopback.Scope.Constants.SetItem(Loopback.Step.ArgumentName, iterator.AsArray().Push(value)), loopback);
    }
}
