using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassAPic.Core.WordManager;
using PassAPic.Models.Models.Models;

namespace PassAPic.Tests
{
    [TestClass]
    public class WordManagerShouldBeAbleTo
    {
        [TestMethod]
        public async Task GetANormalWord()
        {
            var wm = new WordnikManager();
            var result = wm.GetWord(Mode.Normal);
            Assert.IsNotNull(result);
        }
    }

}
