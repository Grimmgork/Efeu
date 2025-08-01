﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;

namespace Efeu.Runtime.Method
{
    public class WorkflowMethodContext
    {
        public readonly SomeData Input;

        public SomeData Output;

        public SomeData Data;

        public string? Route;

        public readonly SomeData DispatchResult;

        public SomeStruct Variables;

        public readonly bool InitialRun;

        public WorkflowMethodContext(SomeStruct variables, SomeData input)
        {
            Variables = variables;
            Input = input;
            Output = new SomeData();
            InitialRun = true;
        }

        public WorkflowMethodContext(SomeStruct variables, SomeData input, SomeData data, SomeData dispatchResult)
        {
            Variables = variables;
            Input = input;
            Data = data;
            Output = new SomeData();
            InitialRun = false;
            DispatchResult = dispatchResult;
        }
    }
}
