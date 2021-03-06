using System.Collections.Generic;
using System.Linq;

namespace Universe.DiskBench
{
    public class ProgressInfo
    {
        public bool IsCompleted { get; set; }
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

            return new ProgressInfo() {IsCompleted = IsCompleted, Steps = steps};
        }

        public ProgressStep LastCompleted => Steps.LastOrDefault(x => x.State == ProgressStepState.Completed);
    }
}