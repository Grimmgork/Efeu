using System;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Entities;

public class ValueNodeRow
{
    public EfeuValueReference Id;

    public EfeuValueReference? ParentId;

    public string? Field;
    
    public EfeuValueReference ValueReference;
    
    public EfeuValueTag Tag;

    public string? Type = "";
    
    public byte[] Payload = [];
}