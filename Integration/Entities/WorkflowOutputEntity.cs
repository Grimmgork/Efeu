namespace Efeu.Integration.Entities
{
    public class WorkflowOutputEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int To { get; set; }
        public uint RunningNumber { get; set; }
    }
}
