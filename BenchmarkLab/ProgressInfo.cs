using System.Collections.Generic;

namespace Universe.DiskBench
{
    public class ProgressInfo
    {
        public List<ProgressStep> Steps { get; private set; }

        public ProgressInfo()
        {
            Steps = new List<ProgressStep>();
        }

        public ProgressInfo Clone()
        {
            List<ProgressStep> steps = new List<ProgressStep>(Steps.Count);
            foreach (var step in Steps)
                steps.Add(step.Clone());

            return new ProgressInfo() {Steps = steps};
        }
    }

    public enum ProgressStepState
    {
        Pending,
        InProgress,
        Completed
    }
}