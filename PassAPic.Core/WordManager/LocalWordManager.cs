using Ninject;
using PassAPic.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Core.WordManager
{
    public class LocalWordManager : IWordManager
    {
        [Inject]
        public IDataContext DataContext { get; set; }

        public Task<Word> GetWord(Mode mode)
        {
            int count;
            int index;
            var startingWord = new Word();

            switch (mode)
            {
                case Mode.Normal:
                    count = DataContext.Word.Count();
                    index = new Random().Next(count);
                    startingWord =
                        DataContext.Word.OrderBy(x => x.Id).Skip(index).Select(x => new Word {RandomWord = x.word}).First();
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
    }
}
