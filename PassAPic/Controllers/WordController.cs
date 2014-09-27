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
                var newWords = model.Words.Split(',').ToList();

                switch (model.Mode)
                {
                    case Mode.Normal:
                        foreach (var word in newWords)
                        {
                            _dataContext.Words.Add(new Word { word = word });
                        }
                        break;
                    case Mode.Easy:
                        foreach (var word in newWords)
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