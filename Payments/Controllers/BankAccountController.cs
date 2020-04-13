using Microsoft.AspNet.Identity.Owin;
using Payments.Filters;
using Payments.ViewModels;
using Payments.ViewModels.BankAccount;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Payments.BLL.Services;
using Payments.BLL.DTO.BankAccount;
using AutoMapper;
using Payments.BLL.Interfaces;
using NLog;

namespace Payments.Controllers
{
    // Контроллер для работы с банковскими счетами
    [LoggedExceptionFilter]
    public class BankAccountController : Controller
    {
        private IBankAccountService bankAccountService;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BankAccountController(IBankAccountService bankAccountService)
        {
            this.bankAccountService = bankAccountService;
        }

        private IUserService UserService
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<IUserService>();
            }
        }

        // Незаблокированные банковские счета конкретного пользователя
        [LoggedExceptionFilter]
        [Authorize(Roles = "client")]
        public async Task<ActionResult> BankAccountsData(string sortOrder)
        {
            string nameCurrentUser = User.Identity.Name;
            string idCurrentUser = await UserService.FindUserIdAsync(nameCurrentUser);

            logger.Info("Клиент {0} запросил информацию о своих незаблокированных банковских счетах",
                idCurrentUser);

            IEnumerable<BankAccountUserDTO> bankAccountsUserDTO = 
                await bankAccountService.BankAccountsData(idCurrentUser);

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
        [LoggedExceptionFilter]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BankAccountsDataForAdmin(string id)
        {
            logger.Info("Администратор запросил информацию о банковских счетах клиента {0}", id);

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
            string idCurrentUser = await UserService.FindUserIdAsync(nameCurrentUser);
            CreatureBankAccountModel model = new CreatureBankAccountModel
            {
                ApplicationUserId = idCurrentUser
            };
            return View(model);
        }

        [Authorize(Roles = "client")]
        [HttpPost]
        public async Task<ActionResult> CreateBankAccount(CreatureBankAccountModel model)
        {
            if(ModelState.IsValid)
            {
                CreatureBankAccountDTO creatureBankAccountDTO = new CreatureBankAccountDTO(model.ApplicationUserId, 
                    model.NumberCard, model.Name);

                await bankAccountService.CreateBankAccount(creatureBankAccountDTO);

                logger.Info("Клиент {0} создал новый банковский счет", model.ApplicationUserId);

                return RedirectToAction("BankAccountsData", "BankAccount");
            }
            return View(model);
        }

        // Возможность администратора заблокировать
        // банковский счет клиента
        //[LoggedExceptionFilter]
        [LoggedExceptionFilter]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BlockBankAccount(int? id)
        {
            await bankAccountService.BlockBankAccount(id);

            logger.Info("Банковский счет {0} заблокирован администратором", id);

            return RedirectToAction("DataUsers", "Account");
        }

        // Возможность администратора разблокировать
        // банковский счет клиента
        [LoggedExceptionFilter]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> UnBlockBankAccount(int? id)
        {
            await bankAccountService.UnBlockBankAccount(id);

            logger.Info("Банковский счет {0} разблокирован администратором", id);

            return RedirectToAction("DataUsers", "Account");
        }

        // Возможность клиента заблокировать
        // свой собственный счет
        [LoggedExceptionFilter]
        [Authorize(Roles = "client")]
        public async Task<ActionResult> BlockSelfBankAccount(int? id)
        {
            await bankAccountService.BlockSelfBankAccount(id);

            logger.Info("Банковский счет {0} заблокирован клиентом", id);

            return RedirectToAction("Security", "BankAccount");
        }

        // Запрос от клиента администратору разблокировать счет
        [LoggedExceptionFilter]
        [Authorize(Roles = "client")]
        public async Task<ActionResult> RequestUnblockBankAccount(int? id)
        {
            await bankAccountService.RequestUnblockBankAccount(id);

            logger.Info("Клиент подал заявку на разблокирование счета {0}", id);

            return RedirectToAction("Security", "BankAccount");
        }

        // Клиент может увидеть данные о заблокированных
        // и незаблокированных банковских счетах
        // При необходимости заблокировать банковский счет
        [LoggedExceptionFilter]
        [Authorize(Roles = "client")]
        public async Task<ActionResult> Security()
        {
            string nameCurrentUser = User.Identity.Name;
            string userId = await UserService.FindUserIdAsync(nameCurrentUser);

            logger.Info("Клиент {0} запросил информацию о своих счетах",
                userId);

            var bankAccountsUserDTO = await bankAccountService.GetAllBankAccountsUserAsync(userId);

            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<BankAccountSecurityDTO,
               BankAccountSecurityModel>()).CreateMapper();
            var bankAccountsUser = mapper.Map<IEnumerable<BankAccountSecurityDTO>,
                IEnumerable<BankAccountSecurityModel>>(bankAccountsUserDTO);

            return View(bankAccountsUser);
        }
    }
}