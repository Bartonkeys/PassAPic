using System.Collections.Generic;
using System.Linq;
using PassAPic.Contracts;
using System.Threading.Tasks;
using PassAPic.Core.WordManager;
using PassAPic.Data;
using PassAPic.Models.Models.Models;
using Word = PassAPic.Models.Models.Word;

namespace PassAPic.Core.WordManager
{
    public interface IWordManager
    {
        IDataContext DataContext { get; set; }

        Task<Word> GetWord(Mode mode, bool isLeastUsedWords, ICollection<Game_Exchange_Words> exchangedWords);

        int IncrementGameCount(string word, Mode mode);
        int IncrementExchangeCount(string word, Mode mode);

        IQueryable<Data.Word> LeastUsedWords();
        IQueryable<Data.EasyWord> LeastUsedEasyWords();

    }
}