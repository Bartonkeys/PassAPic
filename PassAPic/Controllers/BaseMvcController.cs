using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PassAPic.Contracts;

namespace PassAPic.Controllers
{
    public class BaseMvcController : Controller
    {

        protected static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected IDataContext DataContext;
        protected string BaseUrl = System.Configuration.ConfigurationSettings.AppSettings["BaseUrl"];


    }
}