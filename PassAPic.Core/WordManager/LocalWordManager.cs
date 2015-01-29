using Ninject;
using PassAPic.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Models.Models;
using PassAPic.Models.Models.Models;

namespace PassAPic.Core.WordManager
{
    public class LocalWordManager : IWordManager
    {
        [Inject]
        public IDataContext DataContext { get; set; }

        public Task<Word> GetWord(Mode mode, bool isLeastUsedWords)
        {
            int count;
            int index;
            var startingWord = new Word();
            IQueryable<Data.Word> wordPool;

            if (isLeastUsedWords)
            {
                var leastUsedWords = LeastUsedWords();
                wordPool = leastUsedWords;
            }
            else
            {
                wordPool = DataContext.Word;
            }
            
            switch (mode)
            {
                case Mode.Normal:
                    count = wordPool.Count();
                    index = new Random().Next(count);
                    startingWord =
                        wordPool.OrderBy(x => x.Id).Skip(index).Select(x => new Word { RandomWord = x.word }).First();
                    break;
                case Mode.Easy:
                    count = DataContext.EasyWord.Count();
                    index = new Random().Next(count);
                    startingWord =
                        DataContext.EasyWord.OrderBy(x => x.Id).Skip(index).Select(x => new Word { RandomWord = x.Word }).First();

                    break;
            }

            return Task.Run(() => startingWord);
        }

        public IQueryable<Data.Word> LeastUsedWords()
        {
            var minGameCount = DataContext.Word.Min(w => w.games);
            var leastUsedWords = DataContext.Word.Where(w => w.games == minGameCount);
            return leastUsedWords;
        }

        public int IncrementGameCount(string word)
        {
            var wordModel = DataContext.Word.FirstOrDefault(w => w.word.Equals(word));
            int gameCount = -1;
            if (wordModel != null)
            {
                if (wordModel.games != null) gameCount = (int) wordModel.games++;
                DataContext.Commit();
            }

            return gameCount;

        }
    }
}
