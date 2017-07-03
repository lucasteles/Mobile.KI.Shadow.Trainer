using Mobile.KI.Shadow.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile.KI.Shadow.Trainer
{
    public class DataLoader
    {
        public volatile static IList<Character> Characters;

        public static async Task LoadCharactersAsync(Stream fileStream)
        {
            using (var sr = new StreamReader(fileStream))
            {
                try
                {

                    var textJson = await sr.ReadToEndAsync().ConfigureAwait(false);
                    var result = JsonConvert.DeserializeObject<IEnumerable<Character>>(textJson);
                    Characters = result.OrderBy(e => e.Name).ToList();

                }
                catch (Exception ex)
                {

                    throw;
                }
              
            }

        }


    }
}
