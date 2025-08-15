using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Efeu.Runtime.Data
{
    public interface IWorkflowTraversalDescriptor
    {
        public WorkflowTraversalNodeType GetNodeType(object? value);
        public object? TraverseByField(object? value, string field);
        public object? TraversByIndex(object? value, int index);
        public SomeData Parse(object? value);
    }
}
