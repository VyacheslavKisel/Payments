using Microsoft.AspNet.Identity.Owin;
using Payments.Filters;
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
using Service.Repositories;
using Payments.BLL.Services;
using Payments.BLL.DTO.BankAccount;
using AutoMapper;

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

        BankAccountService bankAccountService = new BankAccountService();

        // Незаблокированные банковские счета конкретного пользователя
        [Authorize(Roles = "client")]
        public async Task<ActionResult> BankAccountsData(string sortOrder)
        {
            string nameCurrentUser = User.Identity.Name;
            ApplicationUser currentUser = await UserManager.FindByNameAsync(nameCurrentUser);
            //IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts.
            //    FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUser.Id && bankAccount.LockoutEnabled == false);
            //var bankAccountsUser = bankAccountsAll
            //    .Select(bankAccount => new BankAccountUser(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard, 
            //    bankAccount.Name, bankAccount.Balance))
            //    .ToList();
            IEnumerable<BankAccountUserDTO> bankAccountsUserDTO = 
                await bankAccountService.BankAccountsData(currentUser.Id);
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<BankAccountUserDTO,
                BankAccountUser>()).CreateMapper();
            var bankAccountsUser = mapper.Map<IEnumerable<BankAccountUserDTO>,
                IEnumerable<BankAccountUser>>(bankAccountsUserDTO);
            switch (sortOrder)
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
        // доступны для просмотра и дальнейшей блокировки администратором
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BankAccountsDataForAdmin(string id)
        {
            //IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
            //    .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == id);
            //var bankAccountsUser = bankAccountsAll
            //    .Select(bankAccount => new BankAccountUserDataForAdmin(bankAccount.Id, bankAccount.NumberAccount,
            //    bankAccount.NumberCard, bankAccount.Name, bankAccount.Balance, 
            //    bankAccount.LockoutEnabled, bankAccount.RequestUnblock))
            //    .ToList();
            IEnumerable<BankAccountDataForAdminDTO> bankAccountsUserDTO =
                await bankAccountService.BankAccountsDataForAdmin(id);
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<BankAccountDataForAdminDTO,
                BankAccountUserDataForAdmin>()).CreateMapper();
            var bankAccountsUser = mapper.Map<IEnumerable<BankAccountDataForAdminDTO>,
                IEnumerable<BankAccountUserDataForAdmin>> (bankAccountsUserDTO);
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
                //BankAccount existingNumberBankAccount;
                //string formedNumberBankAccount;
                //do
                //{
                //    existingNumberBankAccount = null;
                //    formedNumberBankAccount = ServiceBankAccount.FormNumberAccount();
                //    existingNumberBankAccount = await database.BankAccounts
                //        .FindAsync(p => p.NumberAccount == formedNumberBankAccount);
                //}
                //while (existingNumberBankAccount != null);
                //BankAccount bankAccount = new BankAccount()
                //{
                //    NumberCard = model.NumberCard,
                //    Name = model.Name,
                //    Balance = 20000,
                //    ApplicationUserId = model.ApplicationUserId,
                //    LockoutEnabled = false,
                //    RequestUnblock = false,
                //    NumberAccount = formedNumberBankAccount
                //};
                //database.BankAccounts.Create(bankAccount);
                //await database.SaveAsync();
                CreatureBankAccountDTO creatureBankAccountDTO = new CreatureBankAccountDTO(model.ApplicationUserId, 
                    model.NumberCard, model.Name);
                await bankAccountService.CreateBankAccount(creatureBankAccountDTO);
                return RedirectToAction("BankAccountsData", "BankAccount");
            }
            return View(model);
        }

        // Возможность администратора заблокировать
        // банковский счет клиента
        [LoggedExceptionFilter]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BlockBankAccount(int? id)
        {
            //BankAccount bankAccount = await database.BankAccounts.GetAsync(id);
            //bankAccount.LockoutEnabled = true;
            //database.BankAccounts.Update(bankAccount);
            //await database.SaveAsync();
            //try
            //{
            //    await bankAccountService.BlockBankAccount(id);
            //}
            //catch (Exception exception)
            //{
            //    return HttpNotFound();
            //}
            await bankAccountService.BlockBankAccount(id);
            return RedirectToAction("DataUsers", "Account");
        }

        // Возможность администратора разблокировать
        // банковский счет клиента
        [LoggedExceptionFilter]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> UnBlockBankAccount(int? id)
        {
            //BankAccount bankAccount = await database.BankAccounts.GetAsync(id);
            //bankAccount.LockoutEnabled = false;
            //bankAccount.RequestUnblock = false;
            //database.BankAccounts.Update(bankAccount);
            //await database.SaveAsync();
            //try
            //{
            //    await bankAccountService.UnBlockBankAccount(id);
            //}
            //catch (Exception exception)
            //{
            //    return HttpNotFound();
            //}
            await bankAccountService.UnBlockBankAccount(id);
            return RedirectToAction("DataUsers", "Account");
        }

        // Возможность клиента заблокировать
        // свой собственный счет
        [LoggedExceptionFilter]
        [Authorize(Roles = "client")]
        public async Task<ActionResult> BlockSelfBankAccount(int? id)
        {
            //BankAccount bankAccount = await database.BankAccounts.GetAsync(id);
            //bankAccount.LockoutEnabled = true;
            //database.BankAccounts.Update(bankAccount);
            //await database.SaveAsync();
            await bankAccountService.BlockSelfBankAccount(id);
            return RedirectToAction("Security", "Home");
        }

        // Запрос клиента администратору
        // разблокировать счет
        [LoggedExceptionFilter]
        [Authorize(Roles = "client")]
        public async Task<ActionResult> RequestUnblockBankAccount(int? id)
        {
            //BankAccount bankAccount = await database.BankAccounts.GetAsync(id);
            //bankAccount.RequestUnblock = true;
            //database.BankAccounts.Update(bankAccount);
            //await database.SaveAsync();
            await bankAccountService.RequestUnblockBankAccount(id);
            return RedirectToAction("Security", "Home");
        }
    }
}