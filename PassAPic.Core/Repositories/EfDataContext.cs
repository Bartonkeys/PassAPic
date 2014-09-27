﻿using System.Data.Entity;
using PassAPic.Contracts;
using PassAPic.Data;

namespace PassAPic.Core.Repositories
{
    public class EfDataContext: IDataContext
    {
        private readonly PassAPicModelContainer _context;
        public EfDataContext()
        {
            _context = new PassAPicModelContainer();
        }

        public IDbSet<User> User { get { return _context.Users; } }
        public IDbSet<Game> Game { get { return _context.Games; } }
        public IDbSet<Guess> Guess { get { return _context.Guesses; } }
        public IDbSet<Word> Word { get { return _context.Words; } }
        public IDbSet<EasyWord> EasyWord { get { return _context.EasyWords; } }
        public IDbSet<PushRegister> PushRegister { get { return _context.PushRegisters; } }
        public void Commit()
        {
            _context.SaveChanges();        
        }
    }
}
