using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Payments.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext _db = new ApplicationContext();
        public ActionResult Index()
        {
            return View();
        }

        // доработать, передается не ViewModel
        [Authorize(Roles = "client")]
        public ActionResult Security()
        {
            return View(_db.BankAccounts.ToList());
        }
    }
}