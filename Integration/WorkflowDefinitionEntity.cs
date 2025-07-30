namespace Efeu.Integration
{
    using Efeu.Runtime.Model;

    public class WorkflowDefinitionEntity
	{
		public int Id { get; set; }

		public string Name { get; set; } = "";

		public WorkflowDefinition Definition { get; set; } = new WorkflowDefinition();
	}
}