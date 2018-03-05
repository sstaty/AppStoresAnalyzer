using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using Windows.Storage;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace App_Stores_Analyzer.HTML_Analyzers
{
    class GooglePlayAppInfo
    {
        public string Name;
        public string Developer;
        public float Rating;
        public string Genre;
        public bool isGenred = false;
        public bool isRated = false;
        public string NumberOfRatings = "Nothing";
        public float Price;
        public bool Adds = false;
        public bool InAppPurch = false;
        public string stars5, stars4, stars3, stars2, stars1;
        public string dateUpdated;
        public string numberOfDownloads;
        public string inAppProducts = "None";

        public string Log()
        {
            return "Name: " + Name + "\n" +
                   "Developer: " + Developer + "\n" +
                   "Rating: " + Rating + "\n" +
                   "Genre: " + Genre + "\n" +
                   "Number of ratings: " + NumberOfRatings + '\n' +
                   "Price: " + Price + " LBR \n" +
                   "Contains adds: " + Adds + '\n' +
                   "Offers in-app Purchases: " + InAppPurch + '\n' +
                   "No. of 5 stars: " + stars5 + "\n" +
                   "No. of 4 stars: " + stars4 + "\n" +
                   "No. of 3 stars: " + stars3 + "\n" +
                   "No. of 2 stars: " + stars2 + "\n" +
                   "No. of 1 stars: " + stars1 + "\n" +
                   "Updated: " + dateUpdated + "\n" +
                   "No. of downloads: " + numberOfDownloads + "\n" +
                   "In-App Procuts: " + inAppProducts + "\n" +
                   "\n";
        }

        public string Log2()
        {
            return        Name + " # " 
                        + Developer + " # " 
                        + Rating + " # " 
                        + Genre + " # " 
                        + NumberOfRatings + " # "
                        + Price + " # "
                        + Adds + " # "
                        + InAppPurch + " # "
                        + stars5 + " # "
                        + stars4 + " # "
                        + stars3 + " # "
                        + stars2 + " # "
                        + stars1 + " # "
                        + dateUpdated + " # "
                        + numberOfDownloads + " # "
                        + inAppProducts 
                        + '\n';
        }

    }

    class GooglePlayHTMLAnalyzer
    {

        public GooglePlayAppInfo AnalyzeHTMLCodeFromString(String code)
        {
            string[] tokens = code.Split();

            var resultAppInfo = new GooglePlayAppInfo();

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Contains("www.w3.org/1999/xhtml"))
                {
                    if (!tokens[i + 1].Contains("en_GB") && !tokens[i + 2].Contains("en_GB") && !tokens[i + 3].Contains("en_GB") && !tokens[i + 4].Contains("en_GB"))
                    {
                        return null;
                    }
                }

                if (tokens[i].Contains("id-app-title"))
                {
                    string result;
                    result = tokens[i + 1].Substring(13);
                    if (!tokens[i + 1].Contains("</div>"))
                    {
                        int z = i + 2;
                        while (!tokens[z].Contains("</div>"))
                        {
                            result += " " + tokens[z];
                            z++;
                        }
                        result += " " + tokens[z].Remove(tokens[z].Length - 6);
                    }
                    else
                    {
                        result = result.Remove(result.Length - 6);
                    }
                    resultAppInfo.Name = result;
                }

                if (tokens[i].Contains("itemprop=\"name\"") && tokens[i - 1].Contains("span"))
                {
                    string result;
                    result = tokens[i].Substring(16);
                    if (!tokens[i].Contains("</span>"))
                    {
                        int z = i + 1;
                        while (!tokens[z].Contains("</span>"))
                        {
                            result += " " + tokens[z];
                            z++;
                        }
                        result += " " + tokens[z].Remove(tokens[z].Length - 7);
                    }
                    else
                    {
                        result = result.Remove(result.Length - 7);
                    }
                    resultAppInfo.Developer = result;
                }

                if (tokens[i].Contains("reviews-num"))
                {
                    string result, result2;
                    result = Regex.Match(tokens[i + 1], @"([0-9]+\,?[0-9]*\,?[0-9]*\,?[0-9]*)").Value;
                    result2 = Regex.Replace(result, ",", "");
                    //resultAppInfo.NumberOfRatings += '\n' + tokens[i] + " Previous: " + tokens[i+1] + " Previous: " + tokens[i + 2];
                    resultAppInfo.NumberOfRatings = result2;
                }

                if (tokens[i].Contains("star-rating-non-editable-container") && !resultAppInfo.isRated)
                {
                    resultAppInfo.Rating = float.Parse(tokens[i + 2]);
                    resultAppInfo.isRated = true;
                }

                if (tokens[i] == "itemprop=\"price\">")
                {
                    string result;
                    result = Regex.Match(tokens[i - 1], @"([-+]?[0-9]*\.?[0-9]+)").Value;
                    resultAppInfo.Price = float.Parse(result);
                }

                if (tokens[i] == "class=\"ads-supported-label-msg\">")
                {
                    resultAppInfo.Adds = true;
                }

                if (tokens[i].Contains("inapp-msg"))
                {
                    resultAppInfo.InAppPurch = true;
                }

                if (tokens[i].Contains("itemprop=\"genre\"") && !resultAppInfo.isGenred)
                {
                    string result;
                    result = tokens[i].Substring(17);
                    if (!result.Contains("</span>"))
                    {
                        int z = i + 1;
                        while (!tokens[z].Contains("</span>"))
                        {
                            result += " " + tokens[z];
                            z++;
                        }
                        result += " " + tokens[z].Remove(tokens[z].Length - 7);
                    }
                    else
                    {
                        result = result.Substring(0, result.Length - 7);
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
            }

            return resultAppInfo;
        }
    }
}
