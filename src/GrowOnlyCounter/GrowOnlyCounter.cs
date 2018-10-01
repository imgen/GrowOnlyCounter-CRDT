using System;
using System.Linq;

namespace GrowOnlyCounter
{
    public class GrowOnlyCounter
    {
        private int[] Payload { get; }

        public int NodeCount => Payload.Length;
        
        /// <summary>
        /// Gets the counters
        /// </summary>
        /// <returns>A copy of the underlying counters so that it won't be modified by outside</returns>
        public int[] GetPayload() => Payload.ToArray();
        
        public GrowOnlyCounter(int nodeCount) => Payload = new int[nodeCount];

        private GrowOnlyCounter(int[] counters) => Payload = counters;

        public void Update(int nodeId)
        {
            Payload[nodeId]++;
        }

        public int Query() => Payload.Sum();

        public GrowOnlyCounter Merge(GrowOnlyCounter counter) => 
            counter.NodeCount != NodeCount?
            throw new ArgumentException("The node counts of the two counters doesn't match", nameof(counter)) :
            new GrowOnlyCounter(
                Payload.Zip(counter.Payload, Math.Max).ToArray()
            );

        public bool Compare(GrowOnlyCounter counter) => counter.NodeCount != NodeCount?
            throw new ArgumentException("The node counts of the two counters doesn't match", nameof(counter)) :
            Payload.Zip(counter.Payload, (xValue, yValue) => xValue <= yValue)
                .All(x => x);
    }
}