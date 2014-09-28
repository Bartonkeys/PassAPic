using PassAPic.Contracts;
using System.Threading.Tasks;

namespace PassAPic.Core.WordManager
{
    public interface IWordManager
    {
        IDataContext DataContext { get; set; }

        Task<Word> GetWord(Mode mode);
    }
}