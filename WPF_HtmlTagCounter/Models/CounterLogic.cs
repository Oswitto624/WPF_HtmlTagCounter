using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WPF_HtmlTagCounter.Models
{
    public class CounterLogic
    {
        private static async Task<Stream> GetSiteContentToStream(string url)
        {
            WebClient client = new WebClient();
            var content = await client.DownloadStringTaskAsync(url);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Default);
            
            await writer.WriteAsync(content);
            await writer.FlushAsync();

            stream.Position = 0;

            return stream;
        }

        private static int TagCounterAsync(Stream SiteStream)
        {
            var tagCount = 0;
            var expectedTag = "a ";
            var intoTag = false;

            using (var reader = new StreamReader(SiteStream))
            {
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
                            stringBuilder.Append(currentString);

                            if (stringBuilder.ToString().StartsWith($"<{expectedTag}"))
                                tagCount++;

                            stringBuilder.Clear();
                        }
                    }              
                }
            }
            return tagCount;
        }
        
        public static async Task<int> StartCounterAsync(string url)
        {            
            return TagCounterAsync(await GetSiteContentToStream(url));
        }
    }
}
