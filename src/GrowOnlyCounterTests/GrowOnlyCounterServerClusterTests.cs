using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GrowOnlyCounter;
using Xunit;

namespace GrowOnlyCounterTests
{
    public class GrowOnlyCounterServerClusterTests
    {
        [Fact]
        public void TestSimpleCase()
        {
            var serverCluster = new GrowOnlyCounterServerCluster(2);
            serverCluster.UpdateNode(0);
            serverCluster.UpdateNode(1);

            var eventualCounter = serverCluster.GetEventualCounter();
            var payload = eventualCounter.GetPayload();

            payload.Length.Should().Be(serverCluster.NodeCount);
            payload[0].Should().Be(1);
            payload[1].Should().Be(1);
        }

        [Fact]
        public void TestParallelUpdates()
        {
            const int nodeCount = 10;
            var serverCluster = new GrowOnlyCounterServerCluster(nodeCount);
            var expectedPayload = new int[nodeCount];
            Parallel.ForEach(Enumerable.Range(0, nodeCount), nodeId =>
                {
                    int random = new Random().Next(2, 10);
                    for (int i = 0; i < random; i++)
                    {
                        serverCluster.UpdateNode(nodeId);
                    }

                    expectedPayload[nodeId] = random;
                });
            
            var eventualCounter = serverCluster.GetEventualCounter();
            var payload = eventualCounter.GetPayload();

            payload.Length.Should().Be(nodeCount);

            payload.SequenceEqual(expectedPayload).Should().BeTrue();
        }

        [Fact]
        public void TestGetEventualCounterInParallel()
        {
            const int nodeCount = 101;
            var serverCluster = new GrowOnlyCounterServerCluster(nodeCount);
            var expectedPayload = new int[nodeCount];
            Parallel.ForEach(Enumerable.Range(0, nodeCount), nodeId =>
            {
                int random = new Random().Next(2, 10);
                for (int i = 0; i < random; i++)
                {
                    serverCluster.UpdateNode(nodeId);
                }

                expectedPayload[nodeId] = random;
            });
            
            var eventualCounter = serverCluster.GetEventualCounterInParallel();
            var payload = eventualCounter.GetPayload();

            payload.Length.Should().Be(nodeCount);

            payload.SequenceEqual(expectedPayload).Should().BeTrue();
        }
    }
}