using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using Windows.Storage;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App_Stores_Analyzer.HTML_Analyzers
{
    class AppStoreAppInfo
    {
        public string Name;
        public string Developer;
        public string currRating;
        public string allRating;
        public string Genre;
        public string currNumberOfRatings = "Nothing";
        public string allNumberOfRatings = "Nothing";
        public string Price;
        public string Size;
        public int langCount;

        public string Log()
        {
            return "Name: " + Name + "\n" +
                   "Developer: " + Developer + "\n" +
                   "Current version rating: " + currRating + "\n" +
                   "All versions rating: " + allRating + "\n" +
                   "Genre: " + Genre + "\n" +
                   "Current version number of ratings: " + currNumberOfRatings + '\n' +
                   "All versions number of ratings: " + allNumberOfRatings + '\n' +
                   "Price: " + Price + " Dollars \n" +
                   "Size: " + Size + " Bytes \n" +
                   "Number of languages: " + langCount + '\n' +
                   "\n";
        }

        public string Log2()
        {
            return    Name + " # "
                    + Developer + " # "
                    + currRating + " # "
                    + allRating + " # "
                    + Genre + " # "
                    + currNumberOfRatings + " # "
                    + allNumberOfRatings + " # "
                    + Price + " # "
                    + Size + " # "
                    + langCount + " # "
                    + '\n';
        }

    }

    class AppStoreHTMLAnalyzer
    {
        HashSet<string> URLmap = new HashSet<string>();

        public async Task LoadURLmap(StreamReader reader)
        {
            string URLmapString = await reader.ReadToEndAsync();
            string[] URLs = URLmapString.Split();

            foreach (string URL in URLs)
            {
                URLmap.Add(URL);
            }
        }

        public AppStoreAppInfo AnalyzeHTMLCodeFromString(String code)
        {
            string[] tokens = code.Split();
            return AnalyzeHTMLCodeFromTokens(tokens);
        }

        public AppStoreAppInfo AnalyzeHTMLCodeFromTokens(String[] tokens)
        {
            var resultAppInfo = new AppStoreAppInfo();

            Frame frame = (Frame)Window.Current.Content;
            MainPage page = (MainPage)frame.Content;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Contains("trackViewUrl"))
                {
                    if (!URLmap.Contains(tokens[i]))
                    {
                        URLmap.Add(tokens[i]);
                        page.AppStoreAddFoundApp(tokens[i]);

                    }
                    else { throw new IOException(); }
                }

                if (tokens[i].Contains("trackCensoredName"))
                {
                    string result;
                    result = tokens[i].Substring(21);
                    if (!tokens[i].Contains("\","))
                    {
                        int z = i + 1;
                        while (!tokens[z].Contains("\","))
                        {
                            result += " " + tokens[z];
                            z++;
                        }
                        result += " " + tokens[z].Remove(tokens[z].Length - 2);
                    }
                    else
                    {
                        result = result.Remove(result.Length - 2);
                    }
                    resultAppInfo.Name = result;
                }

                if (tokens[i].Contains("artistName"))
                {
                    string result;
                    result = tokens[i].Substring(14);
                    if (!tokens[i].Contains("\","))
                    {
                        int z = i + 1;
                        while (!tokens[z].Contains("\","))
                        {
                            result += " " + tokens[z];
                            z++;
                        }
                        result += " " + tokens[z].Remove(tokens[z].Length - 2);
                    }
                    else
                    {
                        result = result.Remove(result.Length - 2);
                    }
                    resultAppInfo.Developer = result;
                }

                if (tokens[i].Contains("primaryGenreName"))
                {
                    string result;
                    result = tokens[i].Substring(20);
                    if (!tokens[i].Contains("\","))
                    {
                        int z = i + 1;
                        while (!tokens[z].Contains("\","))
                        {
                            result += " " + tokens[z];
                            z++;
                        }
                        result += " " + tokens[z].Remove(tokens[z].Length - 2);
                    }
                    else
                    {
                        result = result.Remove(result.Length - 2);
                    }
                    resultAppInfo.Genre = result;
                }

                if (tokens[i].Contains("languageCodesISO2A"))
                {
                    int langCount = 1;

                    int z = i;
                    while (!tokens[z].Contains("],"))
                    {
                        langCount++;
                        z++;
                    }

                    resultAppInfo.langCount = langCount;
                }

                if (tokens[i].Contains("fileSizeBytes")) { resultAppInfo.Size = Regex.Match(tokens[i], @"([-+]?[0-9]*\.?[0-9]+)").Value; }               
                
                if (tokens[i].Contains("userRatingCount")) { resultAppInfo.allNumberOfRatings = Regex.Match(tokens[i], @"([-+]?[0-9]*\.?[0-9]+)").Value; }

                if (tokens[i].Contains("userRatingCountForCurrentVersion")) { resultAppInfo.currNumberOfRatings = Regex.Match(tokens[i], @"([-+]?[0-9]*\.?[0-9]+)").Value; }

                if (tokens[i].Contains("averageUserRatingForCurrentVersion")) { resultAppInfo.currRating = Regex.Match(tokens[i], @"([-+]?[0-9]*\.?[0-9]+)").Value; }

                if (tokens[i].Contains("averageUserRating")) { resultAppInfo.allRating = Regex.Match(tokens[i], @"([-+]?[0-9]*\.?[0-9]+)").Value; }

                if (tokens[i].Contains("\"price\"")) { resultAppInfo.Price = Regex.Match(tokens[i], @"([-+]?[0-9]*\.?[0-9]+)").Value; }



                /*
               
                if (tokens[i].Contains("inapp-msg"))
                {
                    resultAppInfo.InAppPurch = true;
                }
                

                if (tokens[i].Contains("itemprop=\"applicationCategory\"") && !resultAppInfo.isGenred)
                {
                    string result;
                    result = tokens[i].Substring(31);
                    if (!result.Contains("</span>"))
                    {
                        int z = i + 1;
                        while (!tokens[z].Contains("</span>"))
                        {
                            result += " " + tokens[z];
                            z++;
                        }
                        result += " " + tokens[z].Remove(tokens[z].Length-19);
                    }
                    else
                    {
                        result = result.Substring(0, result.Length-19);
                    }
                    resultAppInfo.isGenred = true;
                    resultAppInfo.Genre = result;
                }
                
                if (tokens[i].Contains("rating-bar-container") && tokens[i + 1].Contains("five"))
                {
                    string result, result2;
                    result = Regex.Match(tokens[i + 13], @"([0-9]+\,?[0-9]*\,?[0-9]*\,?[0-9]*)").Value;
                    result2 = Regex.Replace(result, ",", "");
                    resultAppInfo.stars5 = result2;
                }

                if (tokens[i].Contains("rating-bar-container") && tokens[i + 1].Contains("four"))
                {
                    string result, result2;
                    result = Regex.Match(tokens[i + 13], @"([0-9]+\,?[0-9]*\,?[0-9]*\,?[0-9]*)").Value;
                    result2 = Regex.Replace(result, ",", "");
                    resultAppInfo.stars4 = result2;
                }

                if (tokens[i].Contains("rating-bar-container") && tokens[i + 1].Contains("three"))
                {
                    string result, result2;
                    result = Regex.Match(tokens[i + 13], @"([0-9]+\,?[0-9]*\,?[0-9]*\,?[0-9]*)").Value;
                    result2 = Regex.Replace(result, ",", "");
                    resultAppInfo.stars3 = result2;
                }

                if (tokens[i].Contains("rating-bar-container") && tokens[i + 1].Contains("two"))
                {
                    string result, result2;
                    result = Regex.Match(tokens[i + 13], @"([0-9]+\,?[0-9]*\,?[0-9]*\,?[0-9]*)").Value;
                    result2 = Regex.Replace(result, ",", "");
                    resultAppInfo.stars2 = result2;
                }

                if (tokens[i].Contains("rating-bar-container") && tokens[i + 1].Contains("one"))
                {
                    string result, result2;
                    result = Regex.Match(tokens[i + 13], @"([0-9]+\,?[0-9]*\,?[0-9]*\,?[0-9]*)").Value;
                    result2 = Regex.Replace(result, ",", "");
                    resultAppInfo.stars1 = result2;
                }

                if (tokens[i].Contains("datePublished"))
                {
                    string result;
                    result = Regex.Match(tokens[i], @"([0-9]+)").Value;
                    result += " " + tokens[i + 1];
                    result += " " + Regex.Match(tokens[i + 2], @"([0-9]+)").Value;
                    resultAppInfo.dateUpdated = result;
                }

                if (tokens[i].Contains("numDownloads"))
                {
                    string result;
                    result = tokens[i + 1] + " - " + tokens[i + 3];
                    resultAppInfo.numberOfDownloads = result;
                }

                if (tokens[i].Contains("In-app") && tokens[i + 1].Contains("Products"))
                {
                    string result;
                    result = Regex.Match(tokens[i + 3], @"([-+]?[0-9]*\.?[0-9]+)").Value + " - " + Regex.Match(tokens[i + 5], @"([-+]?[0-9]*\.?[0-9]+)").Value;
                    resultAppInfo.inAppProducts = result;
                }
                */
            }

            return resultAppInfo;
        }
    }
}
