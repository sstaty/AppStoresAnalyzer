using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App_Stores_Analyzer.HTML_Crawlers
{
    class AppStoreHTMLCrawler
    {

        public async Task<Tuple<int, int, int, int>> SkipPassedSearchWords(StreamReader reader)
        {
            string linesString = await reader.ReadToEndAsync();
            string[] lines = linesString.Split();

            int searchWordsCount = int.Parse(lines[lines.Length - 4]);
            int downloadedCount = int.Parse(lines[lines.Length - 3]);
            int analyzedCount = int.Parse(lines[lines.Length - 2]);
            int errorsCount = int.Parse(lines[lines.Length - 1]);

            return new Tuple<int, int, int, int>(searchWordsCount, downloadedCount, analyzedCount, errorsCount);
        }

        public string[] SubArray(string[] array, int from, int to)
        {
            int length = to - from + 1;
            string[] returnArray = new string[length];

            for (int i = 0; i < length; i++)
            {
                returnArray[i] = array.ElementAt(from + i);
            }
            return returnArray;
        }

        public async Task<List<string>> FindSearchResultForOneSearchWord(string searchWord)
        {
            Frame frame = (Frame)Window.Current.Content;
            MainPage page = (MainPage)frame.Content;

            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);

            HttpClient http = new System.Net.Http.HttpClient();
            HttpResponseMessage response = await http.GetAsync(@"https://itunes.apple.com/search?country=us&entity=software&limit=200&lang=en_us&term=" + searchWord, cts.Token);
            string HTMLFoundApps = await response.Content.ReadAsStringAsync();

            string[] tokens = HTMLFoundApps.Split();
            List<string> appParts = new List<string>();

            int start = 0;

            for (int i = 1; i < tokens.Length; i++)
            {
                if (tokens[i].Contains("artistViewUrl")) { start = i; }
                if (tokens[i].Contains("\"userRatingCount\""))
                {
                    appParts.Add(string.Join(" ", SubArray(tokens, start, i)));
                    page.AppStoreUpdateDownloaded();
                    page.AppStoreUpdateTime();
                }
            }

            return appParts;
        }
        
    }
}
