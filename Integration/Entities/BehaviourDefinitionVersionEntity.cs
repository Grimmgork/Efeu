namespace Efeu.Integration.Entities
{
    using Efeu.Router;
    using System;

    public class BehaviourDefinitionVersionEntity
	{
        public int Id;

        public int DefinitionId;

        public int Version;

        public BehaviourDefinitionStep[] Steps = [];

        public BehaviourDefinitionStep GetPosition(string position)
        {
            // 0/Do/0/Else/2

            position = position.TrimStart('/');

            if (string.IsNullOrWhiteSpace(position))
                throw new Exception();

            string[] segments = position.Trim().Split("/");
            if (segments.Length % 2 == 0)
                throw new Exception();

            BehaviourDefinitionStep[] steps = Steps;

            int index = int.Parse(segments[0]);
            BehaviourDefinitionStep step = steps[index];
            for (int i = 1; i < segments.Length; i += 2)
            {
                string path = segments[i];
                if (path == "Do")
                {
                    steps = step.Do;
                }
                if (path == "Else")
                {
                    steps = step.Else;
                }

                index = Int32.Parse(segments[i + 1]);
                step = steps[index];
            }

            return step;
        }
    }
}