using System;
using System.Collections.Generic;
using System.Linq;

namespace GrowOnlyCounter
{
    public class GrowOnlyCounterServerCluster
    {
        private readonly GrowOnlyCounterServerNode[] _serverNodes;

        public int NodeCount => _serverNodes.Length;
        
        public GrowOnlyCounterServerCluster(int nodeCount)
        {
            _serverNodes = new GrowOnlyCounterServerNode[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                _serverNodes[i] = new GrowOnlyCounterServerNode(nodeCount, i);
            }
        }

        public void UpdateNode(int nodeId) => _serverNodes[nodeId].Update();

        /// <summary>
        /// Calculates the eventual state of all the nodes which merges all the counters of the nodes
        /// </summary>
        /// <returns>The counter with the merged payload</returns>
        public GrowOnlyCounter GetEventualCounter() =>
            MergeCounters(_serverNodes
                .Select(x => x.Counter));
        
        private GrowOnlyCounter MergeCounters(IEnumerable<GrowOnlyCounter> counters) =>
            counters.Aggregate(
                new GrowOnlyCounter(NodeCount),
                (accumulated, counter) => accumulated.Merge(counter)
            );

        /// <summary>
        /// Calculates the eventual state of all the nodes which merges all the counters of the nodes
        /// in parallel
        /// Due to the fact that Merge function is idempotent, associative and commutative, we can make 
        /// this aggregate process parallel. But since this is simple enough, doing things in parallel
        /// will actually hurt the performance
        /// </summary>
        /// <returns>The counter with the merged payload</returns>
        public GrowOnlyCounter GetEventualCounterInParallel()
        {
            const int parallelThreshold = 100;
            int processorCount = Environment.ProcessorCount;
            var counters = _serverNodes.Select(x => x.Counter).ToArray();
            if (NodeCount < parallelThreshold || processorCount < 2)
                return MergeCounters(counters);

            int index = 0;
            var counterGroups = counters
                .GroupBy(x => index++ % processorCount);

            var countersOfGroups = counterGroups.AsParallel()
                .Select(MergeCounters).ToArray();
            return MergeCounters(countersOfGroups);
        }
    }
}