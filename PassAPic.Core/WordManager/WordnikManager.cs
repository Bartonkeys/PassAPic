using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using PassAPic.Core.WordManager;

namespace PassAPic.WordManager
{
    public class WordnikManager : IWordManager
    {
        public async Task<Word> GetWord(Mode mode)
        {
            switch (mode)
            {
                case Mode.Normal:
                    using (var client = new HttpClient())
                    {

                        var address = new Uri("http://api.wordnik.com:80/v4/words.json/randomWord?hasDictionaryDef=false&minCorpusCount=500000&maxCorpusCount=-1&minDictionaryCount=1&maxDictionaryCount=-1&minLength=5&maxLength=-1&api_key=4d2867228c7945a20d9030f32db0adae6aa5aa8f648f10be5");

                        var response = await client.GetAsync(address);

                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsAsync<Word>();
                        }
                    }
                    break;
                case Mode.Easy:


                    break;
            }
            return new Word();
        }
    }
}