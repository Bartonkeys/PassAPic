using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ninject;
using PassAPic.Contracts;

namespace PassAPic.Controllers
{
    public class BaseController : ApiController
    {
        protected static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected IUnitOfWork UnitOfWork;
    }
}
