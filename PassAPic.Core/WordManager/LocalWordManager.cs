using Ninject;
using PassAPic.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Data;
using PassAPic.Models.Models.Models;
using Word = PassAPic.Models.Models.Word;

namespace PassAPic.Core.WordManager
{
    public class LocalWordManager : IWordManager
    {
        [Inject]
        public IDataContext DataContext { get; set; }

        public Task<Word> GetWord(Mode mode, bool isLeastUsedWords, ICollection<Game_Exchange_Words> exchangedWords)
        {
            int count;
            int index;
            var startingWord = new Word();
           
            switch (mode)
            {
                case Mode.Normal:
                    IQueryable<Data.Word> wordPool = CreateWordPool(isLeastUsedWords, exchangedWords);
            
                    count = wordPool.Count();
                    index = new Random().Next(count);
                    startingWord =
                        wordPool.OrderBy(x => x.Id).Skip(index).Select(x => new Word { RandomWord = x.word }).First();
                    break;
                case Mode.Easy:
                    IQueryable<Data.EasyWord> easyWordPool = CreateEasyWordPool(isLeastUsedWords, exchangedWords);

                    count = easyWordPool.Count();
                    index = new Random().Next(count);
                    startingWord =
                        easyWordPool.OrderBy(x => x.Id).Skip(index).Select(x => new Word { RandomWord = x.Word }).First();

                    break;
            }

            return Task.Run(() => startingWord);
        }

        private IQueryable<Data.Word> CreateWordPool(bool isLeastUsedWords, ICollection<Game_Exchange_Words> exchangedWords)
        {
            IQueryable<Data.Word> wordPool;
            if (isLeastUsedWords)
            {
                var leastUsedWords = LeastUsedWords();
                if (exchangedWords != null)
                {
                    var exchangedWordList = exchangedWords.Select(gameExchangeWord => gameExchangeWord.word).ToList();
                    wordPool = leastUsedWords.Where(w => exchangedWordList.All(e => e != w.word));
                    if (!wordPool.Any())
                    {
                        wordPool = leastUsedWords;
                    }
                }
                else
                {
                    wordPool = leastUsedWords;
                }
            }
            else
            {
                if (exchangedWords != null)
                {
                    var exchangedWordList = exchangedWords.Select(gameExchangeWord => gameExchangeWord.word).ToList();
                    wordPool = DataContext.Word.Where(w => exchangedWordList.All(e => e != w.word));
                    if (!wordPool.Any())
                    {
                        wordPool = DataContext.Word;
                    }
                }
                else
                {
                    wordPool = DataContext.Word;
                }
            }
            return wordPool;
        }

        private IQueryable<Data.EasyWord> CreateEasyWordPool(bool isLeastUsedWords, ICollection<Game_Exchange_Words> exchangedWords)
        {
            IQueryable<Data.EasyWord> wordPool;
            if (isLeastUsedWords)
            {
                var leastUsedWords = LeastUsedEasyWords();
                if (exchangedWords != null)
                {
                    var exchangedWordList = exchangedWords.Select(gameExchangeWord => gameExchangeWord.word).ToList();
                    wordPool = leastUsedWords.Where(w => exchangedWordList.All(e => e != w.Word));
                    if (!wordPool.Any())
                    {
                        wordPool = leastUsedWords;
                    }
                }
                else
                {
                    wordPool = leastUsedWords;
                }
            }
            else
            {
                if (exchangedWords != null)
                {
                    var exchangedWordList = exchangedWords.Select(gameExchangeWord => gameExchangeWord.word).ToList();
                    wordPool = DataContext.EasyWord.Where(w => exchangedWordList.All(e => e != w.Word));
                    if (!wordPool.Any())
                    {
                        wordPool = DataContext.EasyWord;
                    }
                }
                else
                {
                    wordPool = DataContext.EasyWord;
                }
            }
            return wordPool;
        }

        public IQueryable<Data.Word> LeastUsedWords()
        {
            var minGameCount = DataContext.Word.Min(w => w.games);
            var leastUsedWords = DataContext.Word.Where(w => w.games == minGameCount);
            return leastUsedWords;
        }

        public IQueryable<Data.EasyWord> LeastUsedEasyWords()
        {
            var minGameCount = DataContext.EasyWord.Min(w => w.games);
            var leastUsedEasyWords = DataContext.EasyWord.Where(w => w.games == minGameCount);
            return leastUsedEasyWords;
        }

        public int IncrementGameCount(string word, Mode mode)
        {
            int gameCount = -1;

            switch (mode)
            {
                case Mode.Normal:
                    var wordModel = DataContext.Word.FirstOrDefault(w => w.word.Equals(word));
                    if (wordModel != null)
                    {if (wordModel.games != null) gameCount = (int) wordModel.games++;}
                    break;

                case Mode.Easy:
                    var easyWordModel = DataContext.Word.FirstOrDefault(w => w.word.Equals(word));
                    if (easyWordModel != null)
                    {if (easyWordModel.games != null) gameCount = (int)easyWordModel.games++;}
                    break;
            }

            DataContext.Commit();
            
            return gameCount;

        }

        public int IncrementExchangeCount(string word, Mode mode)
        {
            int exchangeCount = -1;

            switch (mode)
            {
                case Mode.Normal:
                    var wordModel = DataContext.Word.FirstOrDefault(w => w.word.Equals(word));
                    if (wordModel != null)
                    {
                        if (wordModel.exchanges != null)
                        {
                            exchangeCount = (int) wordModel.exchanges++;
                        }
                        else
                        {
                            wordModel.exchanges = 1;
                            exchangeCount = 1;
                        }
                    }
                    break;

                case Mode.Easy:
                    var easyWordModel = DataContext.EasyWord.FirstOrDefault(w => w.Word.Equals(word));
                    if (easyWordModel != null)
                    {
                        if (easyWordModel.exchanges != null)
                        {
                            exchangeCount = (int)easyWordModel.exchanges++;
                        }
                        else
                        {
                            easyWordModel.exchanges = 1;
                            exchangeCount = 1;
                        }
                    }
                    break;
            }

            DataContext.Commit();

            return exchangeCount;
        }

    }
}
