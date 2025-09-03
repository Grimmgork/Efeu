using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public enum WorkflowDataType
    {
        Null,
        Int23,
        Int64,
        Single,
        Double,
        Decimal,
        Timestamp,
        Date,
        Time,
        Boolean,
        String,
        Array,
        Struct,
        Reference,
        Fork,

        Anything
    }
}
