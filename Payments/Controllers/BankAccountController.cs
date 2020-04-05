using Microsoft.AspNet.Identity.Owin;
using Payments.ViewModels;
using Payments.ViewModels.BankAccount;
using Payments.Services;
using Service;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Service.Repositories;

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

        private UnitOfWork database;

        public BankAccountController()
        {
            database = new UnitOfWork();
        }

        // Незаблокированные банковские счета конкретного пользователя
        [Authorize(Roles = "client")]
        public async Task<ActionResult> BankAccountsData(string sortOrder)
        {
            string nameCurrentUser = User.Identity.Name;
            ApplicationUser currentUser = await UserManager.FindByNameAsync(nameCurrentUser);
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts.
                FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUser.Id && bankAccount.LockoutEnabled == false);
            var bankAccountsUser = bankAccountsAll
                .Select(bankAccount => new BankAccountUser(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard, 
                bankAccount.Name, bankAccount.Balance))
                .ToList();
            switch(sortOrder)
            {
                case "Number":
                    bankAccountsUser = bankAccountsUser.OrderBy(bankAccountUser => bankAccountUser.NumberAccount).ToList();
                    break;
                case "Number_desc":
                    bankAccountsUser = bankAccountsUser.OrderByDescending(bankAccountUser => bankAccountUser.NumberAccount).ToList();
                    break;
                case "Name":
                    bankAccountsUser = bankAccountsUser.OrderBy(bankAccountUser => bankAccountUser.Name).ToList();
                    break;
                case "Name_desc":
                    bankAccountsUser = bankAccountsUser.OrderByDescending(bankAccountUser => bankAccountUser.Name).ToList();
                    break;
                case "Balance":
                    bankAccountsUser = bankAccountsUser.OrderBy(bankAccountUser => bankAccountUser.Balance).ToList();
                    break;
                case "Balance_desc":
                    bankAccountsUser = bankAccountsUser.OrderByDescending(bankAccountUser => bankAccountUser.Balance).ToList();
                    break;
            }
            return View(bankAccountsUser);
        }

        // Банковские счета конкретного пользователя
        // доступны для просмотра и дальнейшей блокировки админиситратором
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BankAccountsDataForAdmin(string id)
        {
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
                .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == id);
            var bankAccountsUser = bankAccountsAll
                .Select(bankAccount => new BankAccountUserDataForAdmin(bankAccount.Id, bankAccount.NumberAccount,
                bankAccount.NumberCard, bankAccount.Name, bankAccount.Balance, bankAccount.LockoutEnabled))
                .ToList();
            return View(bankAccountsUser);
        }

        // Создать банковский счет
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
        public async Task<ActionResult> CreateBankAccount(CreatureBankAccountModel model)
        {
            if(ModelState.IsValid)
            {
                BankAccount existingNumberBankAccount;
                string formedNumberBankAccount;
                do
                {
                    existingNumberBankAccount = null;
                    formedNumberBankAccount = ServiceBankAccount.FormNumberAccount();
                    existingNumberBankAccount = await database.BankAccounts
                        .FindAsync(p => p.NumberAccount == formedNumberBankAccount);
                }
                while (existingNumberBankAccount != null);
                BankAccount bankAccount = new BankAccount()
                {
                    NumberCard = model.NumberCard,
                    Name = model.Name,
                    Balance = 20000,
                    ApplicationUserId = model.ApplicationUserId,
                    LockoutEnabled = false,
                    NumberAccount = formedNumberBankAccount
                };
                database.BankAccounts.Create(bankAccount);
                await database.SaveAsync();
                return RedirectToAction("BankAccountsData", "BankAccount");
            }
            return View();
        }

        // Возможность администратора заблокировать
        // банковский счет клиента
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BlockBankAccount(int id)
        {
            BankAccount bankAccount = await database.BankAccounts.GetAsync(id);
            bankAccount.LockoutEnabled = true;
            database.BankAccounts.Update(bankAccount);
            await database.SaveAsync();
            return RedirectToAction("DataUsers", "Account");
        }

        // Возможность администратора разблокировать
        // банковский счет клиента
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> UnBlockBankAccount(int id)
        {
            BankAccount bankAccount = await database.BankAccounts.GetAsync(id);
            bankAccount.LockoutEnabled = false;
            database.BankAccounts.Update(bankAccount);
            await database.SaveAsync();
            return RedirectToAction("DataUsers", "Account");
        }

        // Возможность клиента заблокировать
        // свой собственный счет
        [Authorize(Roles = "client")]
        public async Task<ActionResult> BlockSelfBankAccount(int id)
        {
            BankAccount bankAccount = await database.BankAccounts.GetAsync(id);
            bankAccount.LockoutEnabled = true;
            database.BankAccounts.Update(bankAccount);
            await database.SaveAsync();
            return RedirectToAction("Security", "Home");
        }
    }
}