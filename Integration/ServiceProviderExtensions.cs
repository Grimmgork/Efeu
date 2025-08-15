using Efeu.Integration.Logic;
using Efeu.Runtime.Signal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public static class ServiceProviderExtensions
    {
        public static WorkflowTrigger GetHashForSignal(this IServiceProvider provider, object signal)
        {
            //Type signalHasherType = typeof(IWorkflowTriggerHashProvider<>).MakeGenericType(signal.GetType());
            //object? service = provider.GetServices(signalHasherType);

            //MethodInfo method = signalHasherType.GetMethod(nameof(IWorkflowTriggerHashProvider<object>.GetTriggerHash))!;
            //WorkflowTrigger hash = (WorkflowTrigger)method.Invoke(service, [method])!;

            throw new Exception();
        }
    }
}
