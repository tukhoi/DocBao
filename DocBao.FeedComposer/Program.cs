using Davang.Parser.Dto;
using DocBao.ApplicationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Davang.Utilities.Extensions;

namespace DocBao.FeedComposer
{
    class Program
    {
        private const string FEED_DATA_FILE = @"G:\gitdev\DocBao\DocBao.ApplicationServices\Data\Feeds.txt";

        static void Main(string[] args)
        {
            //CreateBackup();
            //AddNewFeed(@"G:\gitdev\DocBao\DocBao.ApplicationServices\Data\dspl.txt");

            var rows = GetInvalidFeedRows();
            rows.ForEach(r => Console.WriteLine(r));

            Console.ReadLine();
        }

        private static void AddNewFeed(string filePath)
        {
            IList<string> newFeeds = new List<string>();

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var streamReader = new StreamReader(fileStream);
                try
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        var parts = line.Split(',');
                        var newLine = string.Format("{0},{1},{2},{3},{4},{5}",
                            Guid.NewGuid(),
                            "b60df1ad-4a0d-48d2-bbaf-d328ef00067a",
                            parts[0],
                            parts[1],
                            "1",
                            "0");
                        Console.WriteLine("create: " + newLine);
                        newFeeds.Add(newLine);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    fileStream.Close();
                    streamReader.Close();
                }
            }

            if (newFeeds.Count == 0) return;

            using (var fileStream2 = new FileStream(filePath + "_generated.txt", FileMode.CreateNew))
            {
                var streamWriter = new StreamWriter(fileStream2);
                try
                {
                    foreach (var line in newFeeds)
                    {
                        streamWriter.WriteLine(line);
                        Console.WriteLine("write: " + line);
                    }

                    streamWriter.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    fileStream2.Close();
                    streamWriter.Close();
                }
            }
        }

        private static void CreateBackup()
        { 
            var backupFile = FEED_DATA_FILE + "_bak";
            File.Copy(FEED_DATA_FILE, backupFile, true);
            Console.WriteLine("Created backup: " + backupFile);
        }

        private static IList<string> GetInvalidFeedRows()
        {
            IList<string> invalidRows = new List<string>();

            using (var stream = new FileStream(AppConfig.FEED_BANK_FILE_NAME, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var feedData = reader.ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (feedData.Length != 6)
                        {
                            invalidRows.Add(feedData[0]);
                        }
                    }
                }
            }

            return invalidRows;
        }
    }
}
