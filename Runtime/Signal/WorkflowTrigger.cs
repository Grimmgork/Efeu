using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efeu.Runtime.Signal
{
    public struct WorkflowTrigger
    {
        public readonly DateTimeOffset NotBefore;

        public readonly WorkflowSignalHash Hash;

        public WorkflowTrigger(DateTimeOffset notBefore, string name, params string[] payloadHashArguments)
        {
            this.NotBefore = notBefore;
            this.Hash = new WorkflowSignalHash(name, payloadHashArguments);
        }

        public WorkflowTrigger(DateTimeOffset notBefore, WorkflowSignalHash signalHash)
        {
            this.NotBefore = notBefore;
            this.Hash = signalHash;
        }

        public WorkflowTrigger(WorkflowSignalHash signalHash)
        {
            this.Hash = signalHash;
        }

        public WorkflowTrigger(string name, params string[] payloadHashArguments)
        {
            this.Hash = new WorkflowSignalHash(name, payloadHashArguments);
        }
    }
}
