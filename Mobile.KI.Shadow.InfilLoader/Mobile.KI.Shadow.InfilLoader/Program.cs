using Mobile.KI.Shadow.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile.KI.Shadow.Loader
{
    class Program
    {

        static readonly string DATA_URL = "http://ki.infil.net/json/data.json";
        static readonly string THUMB_URL = "http://ki.infil.net/images/thumbs/{0}.png";
        static readonly string VIDEO_URL = "http://ki.infil.net/video/{0}.webm";

        static string THUMBS_FOLDER = "thumbs";
        static string VIDEOS_FOLDER = "videos";

        static void Main() => MainAsync().Wait();

        static readonly int THROTTLE = 5;

        static async Task MainAsync()
        {

            var textJson = await DoRequestAsync();
            var characters = await ParseJson(textJson);
            await DownloadAssets(characters);

            Console.WriteLine("Saving parsed character data");
            var parsedData = JsonConvert.SerializeObject(characters);
            File.WriteAllText("data.json", parsedData);

            Console.WriteLine("ALL DONE!!!!");

        }


        static async Task<string> DoRequestAsync()
        {
            var result =  string.Empty;
            Console.WriteLine("Loading Infil Data");
            var http = (HttpWebRequest)WebRequest.Create(DATA_URL);
            http.ContentType = "application/json; charset=utf-8";

            var response =(await http.GetResponseAsync()) as HttpWebResponse;
            using (Stream responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                result = await (reader.ReadToEndAsync());
            }

            return result;
        }

        static async Task<IEnumerable<Character>> ParseJson(string json)
        {
            Console.WriteLine("Parsing json content");
            var charactersArray = JArray.Parse(json);

            var result = new List<Character>();

            foreach (JObject item in charactersArray)
            {
                var type = item.GetValue("type").Value<string>();

                if (type != "character")
                    continue;


                var name = item.GetValue("name").Value<string>();
                var filename = item.GetValue("filename").Value<string>();

                var character = new Character
                {
                    Name = name,
                    Thumb = filename
                };


                var moves = new List<Move>();

                foreach (JObject move in item.GetValue("shadowlinkers"))
                {
                    var newMove = new Move {
                        Name = move.GetValue("name").Value<string>(),
                        Freeze = move.GetValue("freeze").Value<int>(),
                        StartGap = move.GetValue("startgap").Value<int>(),
                        VideoSrc = Path.GetFileName(move.GetValue("videoSrc").Value<string>())
                        
                    };

                    var ranges = new List<Range>();
                    var rawMovedata = move.GetValue("data").Value<string>().Split(' ');
                    foreach (var r in rawMovedata)
                    {
                        var r_type = r.Contains("[") ? TimeRangeType.GAP : TimeRangeType.ACTIVE;

                        var unparsedNumber = "0";
                        unparsedNumber = r_type == TimeRangeType.GAP 
                                ? r.Replace("[", "").Replace("]", "").Trim() 
                                : r.Trim();


                        var range = new Range
                        {
                            Size = int.Parse(unparsedNumber),
                            Type = r_type
                        };


                        ranges.Add(range);

                    }
                    newMove.Ranges = ranges;
                    moves.Add(newMove);
                }

                character.Moves = moves;
                result.Add(character);

            }

            return result;

        }

        static async Task DownloadAssets(IEnumerable<Character> characters)
        {
            Console.WriteLine("Downloading assets...");

            if (!Directory.Exists(THUMBS_FOLDER))
                Directory.CreateDirectory(THUMBS_FOLDER);

            if (!Directory.Exists(VIDEOS_FOLDER))
                Directory.CreateDirectory(VIDEOS_FOLDER);


            var characterDownloadList = new List<Func<Task>>();
            foreach (var chara in characters)
            {
                var thumb_url = string.Format(THUMB_URL, chara.Thumb);
                var thumb_filename = Path.Combine(THUMBS_FOLDER, Path.GetFileName(thumb_url));

                characterDownloadList.Add(
                  () => SingleDownloadAsync(thumb_url, thumb_filename, $"{chara.Name} thumb")
                );

                               
                foreach (var move in chara.Moves)
                {

                    var move_url = string.Format(VIDEO_URL, move.VideoSrc);
                    var move_filename = Path.Combine(VIDEOS_FOLDER, Path.GetFileName(move_url));

                    characterDownloadList.Add(
                        () => SingleDownloadAsync(move_url, move_filename, $"{chara.Name} - {move.Name} move")                   
                   );


                }

            }

            //await Task.WhenAll(characterDownloadList);
            await Observable
                     .Range(0, characterDownloadList.Count()-1)
                     .Select(n => Observable.FromAsync(()=>characterDownloadList[n]() ))
                     .Merge(THROTTLE);

            

        }

        static async Task SingleDownloadAsync(string url, string path, string message = "")
        {
            Console.WriteLine($"Downloading {message}");

            using (var wb = new WebClient())
            {
                wb.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.33 Safari/537.36");
                await wb.DownloadFileTaskAsync( new Uri(url), path);
            }
            Console.WriteLine($"done {message}");
        }

    }

   

}