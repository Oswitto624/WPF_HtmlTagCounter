using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPF_HtmlTagCounter.Models
{
    public class CounterLogic
    {
        private static Stream GetSiteContentToStream(string url, CancellationToken Cancel)
        {
            //Cancel.ThrowIfCancellationRequested();

            WebClient client = new WebClient();
            var content =  client.DownloadString(url);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Default);
            
             writer.WriteAsync(content);
             writer.FlushAsync();

            stream.Position = 0;

            return stream;
        }

        private static Task<int> TagCounterAsync(Stream SiteStream, IProgress<int> Progress, CancellationToken Cancel)
        {
            var tagCount = 0;
            var expectedTag = "a";
            var intoTag = false;

            if (Cancel.IsCancellationRequested)
                return Task.FromCanceled<int>(Cancel);

            using (var reader = new StreamReader(SiteStream))
            {
                var readerPosition = 1;
                var stringBuilder = new StringBuilder();
                var streamLength = SiteStream.Length;
                
                while (!reader.EndOfStream)
                {
                    var currentByte = reader.Read();
                    var currentString = char.ConvertFromUtf32(currentByte);

                    if (readerPosition % 99 == 0) //отладочная задержка
                        Thread.Sleep(1);

                    if (currentString == "<" && !intoTag)
                        intoTag = true;

                    if (intoTag)
                    {
                        stringBuilder.Append(currentString);
                        if (currentString == ">")
                        {
                            var tempString = stringBuilder.ToString();

                            if (tempString.StartsWith($"<{expectedTag}") || tempString.EndsWith($"/{expectedTag}>"))
                                tagCount++;

                            stringBuilder.Clear();
                        }
                    }
                    var progress = (int)Math.Round((double)readerPosition * 100 / streamLength);
                    readerPosition++;
                }
            }
            Progress.Report(100);
            return Task.FromResult(tagCount);
        }
        
        public static Task<int> StartCounterAsync(string Url, IProgress<int> Progress, CancellationToken Cancel)
        {
            var tagCount = TagCounterAsync(GetSiteContentToStream(Url, Cancel), Progress, Cancel);
            return tagCount;
            //return await TagCounterAsync(await GetSiteContentToStream(Url, Cancel), Progress, Cancel);
        }
    }
}
