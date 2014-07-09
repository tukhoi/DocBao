using DocBao.ApplicationServices.Background;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;
using System.Threading;

namespace DocBao.Tests.ApplicationServices.Background
{
    [TestClass]
    public class FeedDownloadRunTests
    {
        [TestMethod]
        public void PriorGroupShouldRunTwiceThanOtherGroup()
        {
            var feedDownloads = new List<MockFeedDownload>();
            for (int i = 0; i < 100; i++)
                feedDownloads.Add(new MockFeedDownload() 
                {
                    Id = i,
                    UpdateTime = i < 20 ? 1 : 2,
                    RunCount = 0
                });

            Run(feedDownloads);

            var runCountStatsGroupA = feedDownloads.Take(20).Sum(fd => fd.RunCount);
            var runCountStatsGroupB = feedDownloads.Skip(20).Take(100 - 20).Sum(fd => fd.RunCount);

            Assert.IsTrue(runCountStatsGroupA == 0.5 * runCountStatsGroupB);
        }

        private void Run(IList<MockFeedDownload> feedDownloads)
        {
            for (int i = 0; i < 10000; i++)
            {
                var feedsToRun = feedDownloads.OrderBy(fd => fd.UpdateTime).Take(10).ToList();
                feedDownloads.ForEach(fd =>
                    {
                        fd.RunCount++;
                        fd.UpdateTime += fd.UpdateTime % 2 == 0 ? 4 : 2;
                    });

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
