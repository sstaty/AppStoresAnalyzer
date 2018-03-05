using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using App_Stores_Analyzer.HTML_Crawlers;
using App_Stores_Analyzer.HTML_Analyzers;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App_Stores_Analyzer
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }


        //________________________________________________________
        //------UNIVERSAL VARIABLES & METHODS------
        StorageFolder WorkingFolder;

        StorageFile SearchWordsFile;
        List<string> SearchWords = new List<string>();

        private async void SetWorkingFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker outputFolderPicker;
            outputFolderPicker = new FolderPicker();
            outputFolderPicker.ViewMode = PickerViewMode.Thumbnail;
            outputFolderPicker.FileTypeFilter.Add(".");
            WorkingFolder = await outputFolderPicker.PickSingleFolderAsync();

            if (WorkingFolder != null)
            {
                OutputTextBlock.Text = WorkingFolder.Path;

                AnalyzeGooglePlayButton.IsEnabled = true;
                AnalyzeAppStoreButton.IsEnabled = true;
                AnalyzeWindowsStoreButton.IsEnabled = true;

                try
                {
                    GooglePlayWorkingFolder = await WorkingFolder.GetFolderAsync("Google Play");
                }
                catch(Exception)
                {
                    GooglePlayWorkingFolder = await WorkingFolder.CreateFolderAsync("Google Play");
                }

                try
                {
                    AppStoreWorkingFolder = await WorkingFolder.GetFolderAsync("App Store");
                }                   
                catch(Exception)
                {
                    AppStoreWorkingFolder = await WorkingFolder.CreateFolderAsync("App Store");
                }

                try
                {
                    WindowsStoreOutputFolder = await WorkingFolder.GetFolderAsync("Windows Store");
                }
                catch(Exception)
                {
                    WindowsStoreOutputFolder = await WorkingFolder.CreateFolderAsync("Windows Store");
                }
                
                

                try
                {
                    SearchWordsFile = await WorkingFolder.GetFileAsync("Search Words List.txt");
                }
                catch(Exception)
                {
                    return;
                }
                
                await ReadSearchWords();
            }
        }

        private async Task ReadSearchWords()
        {
            string searchList = "";

            using (var inputStream = await SearchWordsFile.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                searchList = await streamReader.ReadToEndAsync();
            }

            string[] searchWords = searchList.Split();
            foreach(string searchWord in searchWords)
            {
                if ( !(searchWord == ""))
                {
                    SearchWords.Add(searchWord);
                }
            }
        }








        //________________________________________________________
        //------GOOGLE PLAY------

        //variables:
        StorageFolder GooglePlayWorkingFolder;

        StreamReader GooglePlayFoundURLsReader;
        StreamReader GooglePlayPassedSearchWordsReader;
        
        StreamWriter GooglePlayLogExcelWriter;
        StreamWriter GooglePlayLogHumanWriter;
        StreamWriter GooglePlayPassedSearchWordsWriter;
        StreamWriter GooglePlayFoundURLsWriter;

        int GooglePlaySearchWordsCount = 0;
        int GooglePlayDownloadedCount = 0;
        int GooglePlayAnalyzedCount = 0;
        int GooglePlayErrorsCount = 0;

        Stopwatch StopwatchGooglePlay = new Stopwatch();

        //methods:
        private async Task InitializeGooglePlayFilesReadersAndWriters(GooglePlayHTMLCrawler crawler)
        {
            StorageFile GooglePlayLogExcelFile;
            StorageFile GooglePlayLogHumansFile;
            StorageFile GooglePlayFoundURLsFile;
            StorageFile GooglePlayPassedSearchWordsFile;

            try
            {
                GooglePlayLogExcelFile = await GooglePlayWorkingFolder.GetFileAsync("LogExcel.txt");
            }
            catch(Exception)
            {
                GooglePlayLogExcelFile = await GooglePlayWorkingFolder.CreateFileAsync("LogExcel.txt");
            }

            try
            {
                GooglePlayLogHumansFile = await GooglePlayWorkingFolder.GetFileAsync("LogHumans.txt");
            }
            catch(Exception)
            {
                GooglePlayLogHumansFile = await GooglePlayWorkingFolder.CreateFileAsync("LogHumans.txt");
            }

            bool foundURLs = false;
            try
            {
                //If we found?
                GooglePlayFoundURLsFile = await GooglePlayWorkingFolder.GetFileAsync("GooglePlayFoundURLs.txt");
                foundURLs = true;
            }
            catch(Exception)
            {
                //If we don't
                GooglePlayFoundURLsFile = await GooglePlayWorkingFolder.CreateFileAsync("GooglePlayFoundURLs.txt");
            }

            bool foundPassedSearchWords = false;
            try
            {
                //If we found:
                GooglePlayPassedSearchWordsFile = await GooglePlayWorkingFolder.GetFileAsync("GooglePlayPassedSearchWords.txt");
                foundPassedSearchWords = true;
            }
            catch(Exception)
            {
                //if we don't
                GooglePlayPassedSearchWordsFile = await GooglePlayWorkingFolder.CreateFileAsync("GooglePlayPassedSearchWords.txt");
            }

            //basic write streams:
            var logExcelStream = await GooglePlayLogExcelFile.OpenStreamForWriteAsync();
            var logHumansStream = await GooglePlayLogHumansFile.OpenStreamForWriteAsync();
            var foundURLsStream = await GooglePlayFoundURLsFile.OpenStreamForWriteAsync();
            var passedSearchWordsStream = await GooglePlayPassedSearchWordsFile.OpenStreamForWriteAsync();
            //basic read streams:
            var foundURLsReadStream = await GooglePlayFoundURLsFile.OpenStreamForReadAsync();
            var passedSearchWordsReadStream = await GooglePlayPassedSearchWordsFile.OpenStreamForReadAsync();


            //better write streams:
            var GooglePlayLogExcelStream = logExcelStream.AsOutputStream();
            var GooglePlayLogHumansStream = logHumansStream.AsOutputStream();
            var GooglePlayFoundURLsStream = foundURLsStream.AsOutputStream();
            var GooglePlayPassedSearchWordsStream = passedSearchWordsStream.AsOutputStream();
            //better read streams:
            var GooglePlayFoundURLsReadStream = foundURLsReadStream.AsInputStream();
            var GooglePlayPassedSearchWordsReadStream = passedSearchWordsReadStream.AsInputStream();


            //writers:
            GooglePlayLogExcelWriter = new StreamWriter(GooglePlayLogExcelStream.AsStreamForWrite());
            GooglePlayLogHumanWriter = new StreamWriter(GooglePlayLogHumansStream.AsStreamForWrite());
            GooglePlayFoundURLsWriter = new StreamWriter(GooglePlayFoundURLsStream.AsStreamForWrite());
            GooglePlayPassedSearchWordsWriter = new StreamWriter(GooglePlayPassedSearchWordsStream.AsStreamForWrite());

            //readers:
            GooglePlayFoundURLsReader = new StreamReader(GooglePlayFoundURLsReadStream.AsStreamForRead());
            GooglePlayPassedSearchWordsReader = new StreamReader(GooglePlayPassedSearchWordsReadStream.AsStreamForRead());

            GooglePlayLogExcelWriter.AutoFlush = true;
            GooglePlayLogHumanWriter.AutoFlush = true;
            GooglePlayFoundURLsWriter.AutoFlush = true;
            GooglePlayPassedSearchWordsWriter.AutoFlush = true;

            if(foundURLs)
            {
                await crawler.LoadURLmap(GooglePlayFoundURLsReader);
            }

            if(foundPassedSearchWords)
            {
                Tuple<int, int, int, int> counts = await crawler.SkipPassedSearchWords(GooglePlayPassedSearchWordsReader); //And also loads counts
                GooglePlaySearchWordsCount = counts.Item1;
                GooglePlayDownloadedCount = counts.Item2;
                GooglePlayAnalyzedCount = counts.Item3;
                GooglePlayErrorsCount = counts.Item4;
            }
            
        }

        private async void AnalyzeGooglePlay(object sender, RoutedEventArgs e)
        {
            GooglePlayHTMLCrawler crawler = new GooglePlayHTMLCrawler();

            await InitializeGooglePlayFilesReadersAndWriters(crawler);

            StopwatchGooglePlay.Start();

            for (int i = GooglePlaySearchWordsCount; i < SearchWords.Count; i++)
            {
                GooglePlaySearchWordsTextBlock.Text = (++GooglePlaySearchWordsCount).ToString();
                Tuple<List<string>, List<string>> tuple = await crawler.FindSearchResultForOneSearchWord(SearchWords[i]);
                List<string> htmlStrings = tuple.Item1;
                List<string> urlsStrings = tuple.Item2;

                //write found urls to file:
                foreach(string url in urlsStrings)
                {
                    GooglePlayFoundURLsWriter.WriteLine(url);
                }

                //write counts to passedSearchWords file
                GooglePlayPassedSearchWordsWriter.WriteLine();
                GooglePlayPassedSearchWordsWriter.Write(    GooglePlaySearchWordsCount + " " +
                                                                GooglePlayDownloadedCount + " " +
                                                                GooglePlayAnalyzedCount + " " +
                                                                GooglePlayErrorsCount
                                                            );

                AnalyzeHTMLStringsGooglePlay(htmlStrings);
            }

            StopwatchGooglePlay.Stop();
        }

        private async void AnalyzeHTMLStringsGooglePlay(List<string> htmlStrings)
        {
            var analyzer = new GooglePlayHTMLAnalyzer();

            foreach (string htmlString in htmlStrings)
            {
                try
                {
                    var info = analyzer.AnalyzeHTMLCodeFromString(htmlString);

                    if (info == null)
                    {
                        GooglePlayErrorsTextBlock.Text = (++GooglePlayErrorsCount).ToString();
                    }
                    else
                    {
                        await GooglePlayLogHumanWriter.WriteLineAsync(info.Log());
                        await GooglePlayLogExcelWriter.WriteLineAsync(info.Log2());
                        GooglePlayAnalyzedTextBlock.Text = (++GooglePlayAnalyzedCount).ToString();
                        GooglePlayUpdateTime();
                    }
                }
                catch (Exception)
                {
                    GooglePlayErrorsTextBlock.Text = (++GooglePlayErrorsCount).ToString();
                }
                GooglePlayTimeTextBlock.Text = StopwatchGooglePlay.Elapsed.ToString();
            }

            
            GooglePlayTimeTextBlock.Text = StopwatchGooglePlay.Elapsed.ToString();
        }

        public void GooglePlayUpdateTime()
        {
            GooglePlayTimeTextBlock.Text = StopwatchGooglePlay.Elapsed.ToString();
        }

        public void GooglePlayUpdateDownloaded()
        {
            GooglePlayDownloadedCount++;
            GooglePlayDownloadedTextBlock.Text = GooglePlayDownloadedCount.ToString();
        }






        //________________________________________________________
        //------APP STORE------

        //variables:
        Stopwatch StopwatchAppStore = new Stopwatch();

        StorageFolder AppStoreWorkingFolder;

        StreamReader AppStoreFoundURLsReader;
        StreamReader AppStorePassedSearchWordsReader;

        StreamWriter AppStoreLogExcelWriter;
        StreamWriter AppStoreLogHumanWriter;
        StreamWriter AppStorePassedSearchWordsWriter;
        StreamWriter AppStoreFoundURLsWriter;

        int AppStoreSearchWordsCount = 0;
        int AppStoreDownloadedCount = 0;
        int AppStoreAnalyzedCount = 0;
        int AppStoreErrorsCount = 0;
        int AppStoreCopyCount = 0;

        //methods:
        private async Task InitializeAppStoreFilesReadersAndWriters(AppStoreHTMLCrawler crawler, AppStoreHTMLAnalyzer analyzer)
        {
            StorageFile AppStoreLogExcelFile;
            StorageFile AppStoreLogHumansFile;
            StorageFile AppStoreFoundURLsFile;
            StorageFile AppStorePassedSearchWordsFile;

            try
            {
                AppStoreLogExcelFile = await AppStoreWorkingFolder.GetFileAsync("LogExcel.txt");
            }
            catch (Exception)
            {
                AppStoreLogExcelFile = await AppStoreWorkingFolder.CreateFileAsync("LogExcel.txt");
            }

            try
            {
                AppStoreLogHumansFile = await AppStoreWorkingFolder.GetFileAsync("LogHumans.txt");
            }
            catch (Exception)
            {
                AppStoreLogHumansFile = await AppStoreWorkingFolder.CreateFileAsync("LogHumans.txt");
            }

            bool foundURLs = false;
            try
            {
                //If we found?
                AppStoreFoundURLsFile = await AppStoreWorkingFolder.GetFileAsync("AppStoreFoundURLs.txt");
                foundURLs = true;
            }
            catch (Exception)
            {
                //If we don't
                AppStoreFoundURLsFile = await AppStoreWorkingFolder.CreateFileAsync("AppStoreFoundURLs.txt");
            }

            bool foundPassedSearchWords = false;
            try
            {
                //If we found:
                AppStorePassedSearchWordsFile = await AppStoreWorkingFolder.GetFileAsync("AppStorePassedSearchWords.txt");
                foundPassedSearchWords = true;
            }
            catch (Exception)
            {
                //if we don't
                AppStorePassedSearchWordsFile = await AppStoreWorkingFolder.CreateFileAsync("AppStorePassedSearchWords.txt");
            }

            //basic write streams:
            var logExcelStream = await AppStoreLogExcelFile.OpenStreamForWriteAsync();
            var logHumansStream = await AppStoreLogHumansFile.OpenStreamForWriteAsync();
            var foundURLsStream = await AppStoreFoundURLsFile.OpenStreamForWriteAsync();
            var passedSearchWordsStream = await AppStorePassedSearchWordsFile.OpenStreamForWriteAsync();
            //basic read streams:
            var foundURLsReadStream = await AppStoreFoundURLsFile.OpenStreamForReadAsync();
            var passedSearchWordsReadStream = await AppStorePassedSearchWordsFile.OpenStreamForReadAsync();


            //better write streams:
            var AppStoreLogExcelStream = logExcelStream.AsOutputStream();
            var AppStoreLogHumansStream = logHumansStream.AsOutputStream();
            var AppStoreFoundURLsStream = foundURLsStream.AsOutputStream();
            var AppStorePassedSearchWordsStream = passedSearchWordsStream.AsOutputStream();
            //better read streams:
            var AppStoreFoundURLsReadStream = foundURLsReadStream.AsInputStream();
            var AppStorePassedSearchWordsReadStream = passedSearchWordsReadStream.AsInputStream();


            //writers:
            AppStoreLogExcelWriter = new StreamWriter(AppStoreLogExcelStream.AsStreamForWrite());
            AppStoreLogHumanWriter = new StreamWriter(AppStoreLogHumansStream.AsStreamForWrite());
            AppStoreFoundURLsWriter = new StreamWriter(AppStoreFoundURLsStream.AsStreamForWrite());
            AppStorePassedSearchWordsWriter = new StreamWriter(AppStorePassedSearchWordsStream.AsStreamForWrite());

            //readers:
            AppStoreFoundURLsReader = new StreamReader(AppStoreFoundURLsReadStream.AsStreamForRead());
            AppStorePassedSearchWordsReader = new StreamReader(AppStorePassedSearchWordsReadStream.AsStreamForRead());

            AppStoreLogExcelWriter.AutoFlush = true;
            AppStoreLogHumanWriter.AutoFlush = true;
            AppStoreFoundURLsWriter.AutoFlush = true;
            AppStorePassedSearchWordsWriter.AutoFlush = true;

            if (foundURLs)
            {
                //goes into analyzer
                await analyzer.LoadURLmap(AppStoreFoundURLsReader);
            }

            if (foundPassedSearchWords)
            {
                Tuple<int, int, int, int> counts = await crawler.SkipPassedSearchWords(AppStorePassedSearchWordsReader); //And also loads counts
                AppStoreSearchWordsCount = counts.Item1;
                AppStoreDownloadedCount = counts.Item2;
                AppStoreAnalyzedCount = counts.Item3;
                AppStoreErrorsCount = counts.Item4;
            }

        }

        private async void AnalyzeAppStore(object sender, RoutedEventArgs e)
        {
            AppStoreHTMLCrawler crawler = new AppStoreHTMLCrawler();
            AppStoreHTMLAnalyzer analyzer = new AppStoreHTMLAnalyzer();

            await InitializeAppStoreFilesReadersAndWriters(crawler, analyzer);

            StopwatchAppStore.Start();

            for (int i = AppStoreSearchWordsCount; i < SearchWords.Count; i++)
            {
                AppStoreSearchWordsTextBlock.Text = (++AppStoreSearchWordsCount).ToString();

                List<string> appParts = new List<string>();
                try
                {
                    appParts = await crawler.FindSearchResultForOneSearchWord(SearchWords[i]);
                }
                catch (Exception) { }

                await AnalyzeHTMLStringsAppStore(appParts, analyzer);
                
                //write counts to passedSearchWords file
                AppStorePassedSearchWordsWriter.WriteLine();
                AppStorePassedSearchWordsWriter.Write(AppStoreSearchWordsCount + " " +
                                                                AppStoreDownloadedCount + " " +
                                                                AppStoreAnalyzedCount + " " +
                                                                AppStoreErrorsCount
                                                            );

                
            }

            StopwatchAppStore.Stop();
        }

        private async Task AnalyzeHTMLStringsAppStore(List<string> appParts, AppStoreHTMLAnalyzer analyzer)
        {
            
            foreach (string app in appParts)
            {
                try
                {
                    var info = analyzer.AnalyzeHTMLCodeFromString(app);

                    if (info == null)
                    {
                        AppStoreErrorsTextBlock.Text = (++AppStoreErrorsCount).ToString();
                    }
                    else
                    {
                        await AppStoreLogHumanWriter.WriteLineAsync(info.Log());
                        await AppStoreLogExcelWriter.WriteLineAsync(info.Log2());
                        AppStoreAnalyzedTextBlock.Text = (++AppStoreAnalyzedCount).ToString();         
                    }
                }
                catch (IOException)
                {
                    AppStoreCopyCountTextBlock.Text = (++AppStoreCopyCount).ToString();
                }
                catch (Exception)
                {
                    AppStoreErrorsTextBlock.Text = (++AppStoreErrorsCount).ToString();
                }
                AppStoreUpdateTime();
            }


            AppStoreUpdateTime();



        }

        public void AppStoreUpdateTime()
        {
            AppStoreTimeTextBlock.Text = StopwatchAppStore.Elapsed.ToString();
        }

        public void AppStoreUpdateDownloaded()
        {
            AppStoreDownloadedCount++;
            AppStoreDownloadedTextBlock.Text = AppStoreDownloadedCount.ToString();
        }

        public void AppStoreAddFoundApp(string url)
        {
            AppStoreFoundURLsWriter.WriteLine(url);
        }


        //________________________________________________________
        //------Windows Store------
        StorageFolder WindowsStoreOutputFolder;
    }
}
