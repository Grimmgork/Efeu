﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Function
{
    public class WorkflowValueContext
    {
        public readonly IReadOnlyDictionary<string, SomeData> Variables;

        public readonly IReadOnlyDictionary<int, SomeStruct> MethodOutputs;

        public readonly IReadOnlyDictionary<string, SomeData> WorkflowInputs;

        public Func<int, string, SomeData> getFunctionOutput;

        public WorkflowValueContext(SomeStruct variables, IReadOnlyDictionary<int, SomeStruct> methodOutputs)
        {
            Variables = variables;
            MethodOutputs = methodOutputs;
        }
    }
}
