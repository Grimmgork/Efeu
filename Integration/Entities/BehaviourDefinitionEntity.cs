namespace Efeu.Integration.Entities
{
    using Efeu.Router;
    using Efeu.Runtime.Model;

    public class BehaviourDefinitionEntity
	{
        public int Id;

        public string Name = "";

        public int Version;

        public BehaviourDefinitionStep[] Steps = [];
    }
}