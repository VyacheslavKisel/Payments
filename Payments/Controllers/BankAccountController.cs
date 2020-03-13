using Microsoft.AspNet.Identity.Owin;
using Payments.ViewModels;
using Payments.ViewModels.BankAccount;
using Service;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Payments.Controllers
{
    public class BankAccountController : Controller
    {
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        ApplicationContext _db = new ApplicationContext();

        [Authorize(Roles = "client")]
        public async Task<ActionResult> BankAccountsData()
        {
            string nameCurrentUser = User.Identity.Name;
            ApplicationUser currentUser = await UserManager.FindByNameAsync(nameCurrentUser);
            List<BankAccount> bankAccountsAll = _db.BankAccounts.ToList();
            var bankAccountsUser = bankAccountsAll
                .Where(bankAccount => bankAccount.ApplicationUserId == currentUser.Id && bankAccount.LockoutEnabled == false)
                .Select(bankAccount => new BankAccountUser(bankAccount.Id, bankAccount.NumberCard, 
                bankAccount.Name, bankAccount.Balance))
                .ToList();
            return View(bankAccountsUser);
        }

        [Authorize(Roles = "administrator")]
        public ActionResult BankAccountsDataForAdmin(string id)
        {
            List<BankAccount> bankAccountsAll = _db.BankAccounts.ToList();
            var bankAccountsUser = bankAccountsAll
                .Where(bankAccount => bankAccount.ApplicationUserId == id)
                .Select(bankAccount => new BankAccountUserDataForAdmin(bankAccount.Id, bankAccount.NumberCard,
                bankAccount.Name, bankAccount.Balance, bankAccount.LockoutEnabled))
                .ToList();
            return View(bankAccountsUser);
        }

        //ViewBag
        [Authorize(Roles = "client")]
        [HttpGet]
        public async Task<ActionResult> CreateBankAccount()
        {
            string nameCurrentUser = User.Identity.Name;
            ApplicationUser currentUser = await UserManager.FindByNameAsync(nameCurrentUser);
            ViewBag.UserId = currentUser.Id;
            return View();
        }

        [Authorize(Roles = "client")]
        [HttpPost]
        public ActionResult CreateBankAccount(CreatureBankAccountModel model)
        {
            if(ModelState.IsValid)
            {
                BankAccount bankAccount = new BankAccount()
                {
                    NumberCard = model.NumberCard,
                    Name = model.Name,
                    Balance = 20000,
                    ApplicationUserId = model.ApplicationUserId,
                    LockoutEnabled = false
                };
                _db.BankAccounts.Add(bankAccount);
                _db.SaveChanges();
                return RedirectToAction("BankAccountsData", "BankAccount");
            }
            return View();
        }

        // Лучше, чтобы оставалось в списке счетов
        // а не переходило в список пользователей
        [Authorize(Roles = "administrator")]
        public ActionResult BlockBankAccount(int id)
        {
            BankAccount bankAccount = _db.BankAccounts.Find(id);
            bankAccount.LockoutEnabled = true;
            _db.Entry(bankAccount).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("DataUsers", "Account");
        }

        // Лучше, чтобы оставалось в списке счетов
        // а не переходило в список пользователей
        [Authorize(Roles = "administrator")]
        public ActionResult UnBlockBankAccount(int id)
        {
            BankAccount bankAccount = _db.BankAccounts.Find(id);
            bankAccount.LockoutEnabled = false;
            _db.Entry(bankAccount).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("DataUsers", "Account");
        }

        // Этот метод и метод выше имеют один и тот же код
        [Authorize(Roles = "client")]
        public ActionResult BlockSelfBankAccount(int id)
        {
            BankAccount bankAccount = _db.BankAccounts.Find(id);
            bankAccount.LockoutEnabled = true;
            _db.Entry(bankAccount).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("Security", "Home");
        }
    }
}