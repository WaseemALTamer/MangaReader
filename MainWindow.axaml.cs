using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;
using Avalonia.Threading;



namespace MangaReader
{
    public partial class MainWindow : Window
    {



        TextBlock test;

        HomeWindow FirstWindow;



        MangaContent[] MangasData;

        public MainWindow(){
            InitializeComponent();
        }

        private void OnLoaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FirstWindow = new HomeWindow(this);
            Content = FirstWindow;

            Task.Run(() => LoadContent()); //this will load the content on a thread
        }



        //this is where the data gets loaded
        private void LoadContent() {

            string directoryPath = "Manga";

            if (Directory.Exists(directoryPath)){
                //Graps all the folers
                string[] Folders = Directory.GetDirectories(directoryPath);
                MangasData = new MangaContent[Folders.Length];

                PassContent();


                int _indexManga = 0;
                foreach (string _folder in Folders) {
                    MangasData[_indexManga].FolderName = Path.GetFileName(_folder);
                    string[] _chaptersDir = Directory.GetDirectories(_folder);

                    //Grps all the Chapters
                    MangasData[_indexManga].Chapters = new int[_chaptersDir.Length];


                    //Append the Chapters
                    int _indexChapters = 0;
                    foreach (string _chapter in _chaptersDir) {
                        MangasData[_indexManga].Chapters[_indexChapters] = Int32.Parse(Path.GetFileName(_chapter));
                        _indexChapters += 1;
                    }
                    


                    //Graps the Image
                    string _imagePath = $"{_folder}/Cover.jpg";
                    if (File.Exists(_imagePath))
                    {
                        using (var stream = File.OpenRead(_imagePath)) // we use the "using" so memory leaks does not occure
                        {
                            Bitmap bitmap = new Bitmap(stream);

                            int _localIndex = _indexManga;
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                
                                Image imageControl = new Image
                                {
                                    Source = bitmap,
                                    Stretch = Avalonia.Media.Stretch.Fill
                                };
                                MangasData[_localIndex].CoverImage = imageControl;
                            });

                            
                        }
                    }

                    //Graps the JsonFile For Names
                    string _jsonFilePath = $"{_folder}/Data.json";
                    if (File.Exists(_jsonFilePath))
                    {
                        string _jsonContent = File.ReadAllText(_jsonFilePath);
                        var _data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(_jsonContent);

                        if (_data.ContainsKey("s")) {
                            MangasData[_indexManga].MangaName = _data["s"].GetString();
                        }

                        if (_data.ContainsKey("a"))
                        {
                            var _altirnativeNamesArray = _data["a"].EnumerateArray()
                                .Select(element => element.GetString()) // Convert each JsonElement to a string
                                .ToArray(); // Convert to a string array

                            MangasData[_indexManga].AltirnativeNames = _altirnativeNamesArray;
                        }
                    }

                    //test.Text = $"{MangasData[0].AltirnativeNames[0]}";

                    //Graps the JsonFile For BookMakrs
                    _jsonFilePath = $"{_folder}/BookMark.json";
                    if (File.Exists(_jsonFilePath))
                    {
                        string _jsonContent = File.ReadAllText(_jsonFilePath);
                        var _data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(_jsonContent);

                        if (_data.ContainsKey("Chapter") && _data["Chapter"].ValueKind != JsonValueKind.Null)
                        {

                            MangasData[_indexManga].BookMarkedChapter = _data["Chapter"].GetInt32();
                        }

                        if (_data.ContainsKey("Page") && _data["Page"].ValueKind != JsonValueKind.Null)
                        {
                            MangasData[_indexManga].BookMarkedPage = _data["Page"].GetInt32();
                        }

                        if (_data.ContainsKey("ReadChapters"))
                        {
                            var _readChapters = _data["ReadChapters"].EnumerateArray()
                                .Select(element => element.GetInt32())
                                .ToArray();

                            MangasData[_indexManga].ChaptersRead = _readChapters;
                        }
                    }

                    //End of Content Grapping
                    _indexManga += 1;
                }
            }

        }
        void PassContent()
        {
            FirstWindow.PassContent(MangasData);
        }
    }
}