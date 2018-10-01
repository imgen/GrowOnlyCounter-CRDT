using System;

namespace GrowOnlyCounter
{
    public class GrowOnlyCounterServerNode
    {
        public GrowOnlyCounter Counter { get; }
        public int NodeId { get; }

        public GrowOnlyCounterServerNode(int nodeCount, int nodeId)
        {
            if (nodeId > nodeCount)
                throw new ArgumentOutOfRangeException(nameof(nodeId));
            NodeId = nodeId;
            Counter = new GrowOnlyCounter(nodeCount);
        }

        public void Update() => Counter.Update(NodeId);

        public int Query() => Counter.Query();
    }
}