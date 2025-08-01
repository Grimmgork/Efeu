﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Method;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime.Function
{
    public abstract class WorkflowFunctionInstanceBase : IWorkflowFunctionInstance
    {
        public virtual SomeData Run(WorkflowFunctionContext context, SomeData input)
        {
            throw new NotImplementedException();
        }
    }
}
