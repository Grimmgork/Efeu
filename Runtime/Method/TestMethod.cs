using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class TestMethod : MethodBase
    {
        public override MethodDescription Describe(SomeStruct metaInputs)
        {
            return new MethodDescription()
            {
                Name = "Test",
                Description = "Asdf",
                MetaInputs = [
                    new () {

                    }
                ],
                Inputs = [
                    new () {
                        Name = "A",
                        Description = "adadw",
                        Shape = new SomeDataShape() {
                            Type = WorkflowDataType.Boolean
                        }
                    }
                ]
            };
        }

        public override WorkflowMethodState Run(WorkflowMethodContext context)
        {
            return WorkflowMethodState.Suspended;
        }
    }

    public abstract class MethodBase : IMethodDescriptor
    {
        public virtual MethodDescription Describe(SomeStruct metaInput)
        {
            throw new NotImplementedException();
        }

        public virtual WorkflowMethodState Run(WorkflowMethodContext context)
        {
            return WorkflowMethodState.Done;
        }
    }

    public class MethodDescription
    {
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public ArgumentDescription[] Inputs { get; set; } = [];

        public ArgumentDescription[] Outputs { get; set; } = [];

        public ArgumentDescription[] MetaInputs { get; set; } = [];
    }

    public class SomeDataShape
    {
        public bool IsArray { get; set; }
        public bool IsNullable { get; set; }
        public WorkflowDataType Type { get; set; }
        public string StructType { get; set; }

        public static SomeDataShape Anything()
        {
            return new SomeDataShape()
            {
                IsNullable = true,
                IsArray = true,
                Type = WorkflowDataType.Anything
            };
        }

        // *
        // *[]
        // Type
        // Type?
        // Type[]
        // Struct:SpecificShape
        // Struct:_GUID (Anonymous struct)
        // Struct (Some Struct)
    }

    public class ArgumentDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SomeDataShape Shape { get; set; }
    }

    public interface IMethodDescriptor
    {
        public MethodDescription Describe(SomeStruct metaInput);

        public WorkflowMethodState Run(WorkflowMethodContext context);
    }
}
