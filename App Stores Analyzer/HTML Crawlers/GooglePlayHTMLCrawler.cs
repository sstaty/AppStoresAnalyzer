using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App_Stores_Analyzer.HTML_Crawlers
{
    class GooglePlayHTMLCrawler
    {
        HashSet<string> URLmap = new HashSet<string>();

        public async Task LoadURLmap(StreamReader reader)
        {
            string URLmapString = await reader.ReadToEndAsync();
            string[] URLs = URLmapString.Split();

            foreach(string URL in URLs)
            {
                URLmap.Add(URL);
            }
        }

        public async Task<Tuple<int,int,int,int>> SkipPassedSearchWords(StreamReader reader)
        {
            string linesString = await reader.ReadToEndAsync();
            string[] lines = linesString.Split();

            int searchWordsCount = int.Parse(lines[lines.Length - 4]);
            int downloadedCount = int.Parse(lines[lines.Length - 3]);
            int analyzedCount = int.Parse(lines[lines.Length - 2]);
            int errorsCount = int.Parse(lines[lines.Length - 1]);

            return new Tuple<int, int, int, int>(searchWordsCount, downloadedCount, analyzedCount, errorsCount);
        }

        public async Task<Tuple<List<string>, List<string>>> FindSearchResultForOneSearchWord(string searchWord)
        {
            Frame frame = (Frame)Window.Current.Content;
            MainPage page = (MainPage)frame.Content;

            HttpClient http = new System.Net.Http.HttpClient();
            HttpResponseMessage response = await http.GetAsync(@"https://play.google.com/store/search?q=" + searchWord + "&c=apps&hl=en_gb");
            string HTMLFoundApps = await response.Content.ReadAsStringAsync();

            //Analyze Search Result Page:
            String[] tokens = HTMLFoundApps.Split();
            List<string> urls;

            //Hladanie:
            urls = FoundAppsUrlsInSearchResult(tokens);

            List<string> appsHTMLs = new List<string>();
            foreach (string url in urls)
            {
                HttpResponseMessage response2 = await http.GetAsync(url);
                string htmlString = await response2.Content.ReadAsStringAsync();
                appsHTMLs.Add(htmlString);
                page.GooglePlayUpdateDownloaded();
                page.GooglePlayUpdateTime();
            }

            return new Tuple<List<string>, List<string>>(appsHTMLs, urls);
        }

        public List<string> FoundAppsUrlsInSearchResult(String[] tokens)
        {
            List<string> foundAppsUrls = new List<string>();
            for (int i = 0; i < tokens.Length; i++)
            {
                string url;
                if (tokens[i].Contains("card-click-target") && tokens[i+1].Contains("href"))
                {
                    string result;
                    result = tokens[i + 1].Substring(7);
                    result = result.Remove(result.Length - 1);
                    url = "https://play.google.com/" + result + "&hl=en_GB";

                    if(! URLmap.Contains(url))
                    {
                        URLmap.Add(url);
                        foundAppsUrls.Add(url);
                    }   
                }
            }
            return foundAppsUrls;
        }
    }
}
