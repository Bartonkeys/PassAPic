using System.Threading.Tasks;

namespace PassAPic.Core.WordManager
{
    public interface IWordManager
    {
        Task<Word> GetWord(Mode mode);
    }
}