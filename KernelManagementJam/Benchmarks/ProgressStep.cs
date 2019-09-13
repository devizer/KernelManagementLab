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
        public ProgressStepHistoryColumn Column { get; set; }
        public object Value { get; set; }
        
        public bool CanHaveMetrics { get; set; }
        
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

        // It is ok to start the same step multiple time.
        // On first the UI is updated
        // On last the timer start moment is updated
        public void Start()
        {
            State = ProgressStepState.InProgress;
            StartAt = Stopwatch.StartNew();
        }

        public void Complete()
        {
            State = ProgressStepState.Completed;
            // Seconds = StartAt.ElapsedMilliseconds / 1000d;
            StartAt.Stop();
        }

        public void Progress(double perCents, long bytes)
        {
            PerCents = perCents;
            Seconds = Math.Max((double) StartAt.ElapsedTicks / Stopwatch.Frequency, 0.001f);
            Bytes = bytes;
        }

        public double? AvgBytesPerSecond
        {
            get
            {
                var s = Seconds.GetValueOrDefault();
                return s > 0 ? Bytes / s : (double?) null;
            }
        }
        
        


        public string Duration
        {
            get
            {
                // More details, doesnt work for AsJson comparer for tests
                // var seconds = StartAt?.Elapsed.TotalSeconds ?? Seconds; 
                // Less details, works for AsJson comparer for tests
                var seconds =  Seconds;
                return !seconds.HasValue
                    ? null
                    : new DateTime(0).Add(TimeSpan.FromSeconds(seconds.Value)).ToString("HH:mm:ss");
            }
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
                CanHaveMetrics = CanHaveMetrics,
                // ?
                Value = Value,
                Column = Column,
            };
        }
    }

    public enum ProgressStepHistoryColumn
    {
        Ignore,
        CheckODirect,
        Allocate,
        SeqWrite,
        SeqRead,
        RandRead1T,
        RandWrite1T,
        RandReadNT,
        RandWriteNT,
    }
}