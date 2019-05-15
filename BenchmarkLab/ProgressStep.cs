using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Universe.DiskBench
{
    public class ProgressStep
    {
        [JsonIgnore]
        public Stopwatch StartAt { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ProgressStepState State { get; set; }
        
        public string Name { get; set; }
        public double? Seconds { get; set; }
        public double? PerCents { get; set; }
        public long Bytes { get; set; }
        

        public ProgressStep()
        {
        }

        public ProgressStep(string name)
        {
            Name = name;
        }

        public void Start()
        {
            State = ProgressStepState.InProgress;
            StartAt = Stopwatch.StartNew();
        }

        public void Complete()
        {
            State = ProgressStepState.Completed;
            StartAt.Stop();
        }

        public void Progress(double perCents, long bytes)
        {
            PerCents = perCents;
            Seconds = StartAt.ElapsedMilliseconds / 1000d;
            Bytes += bytes;
        }


        public string Duration
        {
            get { return new DateTime(0).Add(StartAt.Elapsed).ToString("HH:mm:ss"); }
        }
        
        public double? ETA
        {
            get
            {
                var seconds = Seconds;
                var perCents = PerCents;
                if (State == ProgressStepState.InProgress && Seconds > 0 && PerCents > 0)
                {
                    return seconds / perCents - seconds;
                }

                return null;
            }
        }

        public ProgressStep Clone()
        {
            return new ProgressStep()
            {
                State = State,
                StartAt = StartAt,
                PerCents = PerCents,
                Bytes = Bytes,
                Seconds = Seconds,
                Name = Name,
            };
        }
    }
}