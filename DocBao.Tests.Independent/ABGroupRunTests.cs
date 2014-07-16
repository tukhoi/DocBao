using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocBao.Tests.Independent
{
    [TestFixture]
    public class ABGroupRunTests
    {
        [Test]
        public void PriorGroupShouldRunTwiceThanOtherGroup()
        {
            var itemCount = 10;
            var runTime = 100;

            var feedDownloads = new List<MockFeedDownload>();
            for (int i = 0; i < itemCount; i++)
                feedDownloads.Add(new MockFeedDownload()
                {
                    Id = i,
                    UpdateTime = i < itemCount/2 ? 1 : 2,
                    RunCount = 0
                });

            Run(feedDownloads, runTime);

            var runCountStatsGroupA = feedDownloads.Take(itemCount/2).Sum(fd => fd.RunCount);
            var runCountStatsGroupB = feedDownloads.Skip(itemCount/2).Take(itemCount - itemCount/2).Sum(fd => fd.RunCount);

            Console.WriteLine("Group A: " + runCountStatsGroupA);
            Console.WriteLine("Group B: " + runCountStatsGroupB);

            Assert.IsTrue(runCountStatsGroupA > 0.5 * runCountStatsGroupB);
        }

        private void Run(IList<MockFeedDownload> feedDownloads, int runTime)
        {
            for (int i = 0; i < runTime; i++)
            {
                var feedsToRun = feedDownloads.OrderBy(fd => fd.UpdateTime).Take(2).ToList();
                foreach (MockFeedDownload fd in feedsToRun)
                {
                    fd.RunCount++;
                    fd.UpdateTime += fd.UpdateTime % 2 == 0 ? 4 : 2;
                }

                //Thread.Sleep(500);
            }
        }
    }

    class MockFeedDownload
    {
        public int Id { get; set; }
        public int UpdateTime { get; set; }
        public int RunCount { get; set; }
    }
}
