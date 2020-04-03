using Microsoft.AspNet.Identity.Owin;
using Payments.ViewModels.BankAccount;
using Service;
using Service.Models;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Payments.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private UnitOfWork database;

        public HomeController()
        {
            database = new UnitOfWork();
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "client")]
        public async Task<ActionResult> Security()
        {
            string nameCurrentUser = User.Identity.Name;
            ApplicationUser currentUser = await UserManager.FindByNameAsync(nameCurrentUser);
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts.
                FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUser.Id);
            var bankAccountsUser = bankAccountsAll
                .Select(bankAccount => new BankAccountSecurityModel(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard,
                bankAccount.Name, bankAccount.Balance, bankAccount.LockoutEnabled))
                .ToList();
            return View(bankAccountsUser);
        }
    }
}