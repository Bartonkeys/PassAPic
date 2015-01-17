using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.WordManager;
using PassAPic.Data;
using PassAPic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PassAPic.Models.Models;
using PassAPic.Models.Models.Models;
using Word = PassAPic.Data.Word;

namespace PassAPic.Controllers
{
    public class WordController : Controller
    {
        private PassAPicModelContainer _dataContext;

        public WordController()
        {
            _dataContext = new PassAPicModelContainer();
        }
        // GET: Word
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(NewWordsModel model)
        {
            if (ModelState.IsValid && model.Password == "Y)rm91234")
            {
                var newWordsUntrimmed = model.Words.Split(',').ToList();
                var newWordsTrimmed = newWordsUntrimmed.Select(s => s.Trim());
                switch (model.Mode)
                {
                    case Mode.Normal:
                        var existingWords = _dataContext.Words.ToList();
                        var nonDuplicateWords = newWordsTrimmed.Where(x => !existingWords.Any(y => x == y.word));
                        foreach (var word in nonDuplicateWords)
                        {
                            _dataContext.Words.Add(new Word { word = word });
                        }
                        break;
                    case Mode.Easy:

                        var existingWordsEasy = _dataContext.EasyWords.ToList();
                        var nonDuplicateWordsEasy = newWordsTrimmed.Where(x => !existingWordsEasy.Any(y => x == y.Word));
       
                        foreach (var word in nonDuplicateWordsEasy)
                        {
                            _dataContext.EasyWords.Add(new EasyWord { Word = word });
                        }
                        break;
                }

                _dataContext.SaveChanges();
            }

            return View(model);
        }
    }
}