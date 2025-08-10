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
        Integer,
        Long,
        Single,
        Double,
        Decimal,
        Timestamp,
        Boolean,
        String,
        Stream,
        Exception,
        Array,
        Struct,

        Anything
    }
}
