﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Data;
using PassAPic.Models.Models.Models;
using Word = PassAPic.Models.Models.Word;

namespace PassAPic.Core.WordManager
{
    public class WordnikManager : IWordManager
    {
        [Inject]
        public IDataContext DataContext { get; set; }

        public async Task<Word> GetWord(Mode mode, bool isLeastUsedWords,
            ICollection<Game_Exchange_Words> exchangedWords)
        {
            switch (mode)
            {
                case Mode.Normal:
                    using (var client = new HttpClient())
                    {

                        var address =
                            new Uri(
                                "http://api.wordnik.com:80/v4/words.json/randomWord?hasDictionaryDef=true&minCorpusCount=500000&maxCorpusCount=-1&minDictionaryCount=10&maxDictionaryCount=-1&minLength=5&maxLength=-1&includePartOfSpeech=noun&excludePartOfSpeech=adjective&api_key=4d2867228c7945a20d9030f32db0adae6aa5aa8f648f10be5");

                        var response = await client.GetAsync(address);

                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsAsync<Word>();
                        }
                    }
                    break;
                case Mode.Easy:
                    var count = DataContext.EasyWord.Count();
                    var index = new Random().Next(count);
                    var startingWord =
                        DataContext.EasyWord.OrderBy(x => x.Id)
                            .Skip(index)
                            .Select(x => new Word {RandomWord = x.Word})
                            .First();
                    return await Task.Run(() => startingWord);
            }
            return new Word();
        }

        public int IncrementGameCount(string word, Mode mode)
        { throw new NotImplementedException(); }
        public int IncrementExchangeCount(string word, Mode mode)
        { throw new NotImplementedException(); }


        public IQueryable<Data.Word> LeastUsedWords()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Data.EasyWord> LeastUsedEasyWords()
        {
            throw new NotImplementedException();
        }
    }
}