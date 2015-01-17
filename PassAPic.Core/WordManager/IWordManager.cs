using PassAPic.Contracts;
using System.Threading.Tasks;
using PassAPic.Core.WordManager;
using PassAPic.Models.Models;
using PassAPic.Models.Models.Models;

namespace PassAPic.Core.WordManager
{
    public interface IWordManager
    {
        IDataContext DataContext { get; set; }

        Task<Word> GetWord(Mode mode);
    }
}