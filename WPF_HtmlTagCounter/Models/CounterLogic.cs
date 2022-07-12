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
        private static async Task<Stream> GetSiteContentToStream(string url, CancellationToken Cancel)
        {
            Cancel.ThrowIfCancellationRequested();

            WebClient client = new WebClient();
            var content = await client.DownloadStringTaskAsync(url);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Default);
            
            await writer.WriteAsync(content);
            await writer.FlushAsync();

            stream.Position = 0;

            return stream;
        }

        private static Task<int> TagCounterAsync(Stream SiteStream, CancellationToken Cancel)
        {
            var tagCount = 0;
            var expectedTag = "a";
            var intoTag = false;

            using (var reader = new StreamReader(SiteStream))
            {
                Cancel.ThrowIfCancellationRequested();

                var readerPosition = 0;
                var stringBuilder = new StringBuilder();

                while (!reader.EndOfStream)
                {
                    var currentByte = reader.Read();
                    var currentString = char.ConvertFromUtf32(currentByte);

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
                    readerPosition++;
                }
            }
            
            return Task.FromResult(tagCount);
        }
        
        public static async Task<int> StartCounterAsync(string url, CancellationToken Cancel)
        {
            return await TagCounterAsync(await GetSiteContentToStream(url, Cancel), Cancel);
        }
    }
}
