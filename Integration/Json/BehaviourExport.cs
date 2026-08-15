using System;
using System.Text.Json.Serialization.Metadata;
using Efeu.Runtime;

namespace Efeu.Integration.Json;

public class BehaviourExport
{
    public Guid Id { get; set; }

    public int Version { get; set; }

    public string Name { get; set; } = "";

    public EfeuBehaviourStep[] Steps { get; set; } = [];
}