using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using Payments.BLL.DTO.Payment;
using Payments.BLL.Infrastructure;
using Payments.BLL.Interfaces;
using Payments.BLL.Services;
using Payments.Filters;
using Payments.ViewModels;
using Payments.ViewModels.BankAccount;
using Payments.ViewModels.Payment;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Payments.Controllers
{
    // Контроллер для работы с платежами
    [Authorize(Roles = "client")]
    public class PaymentController : Controller
    {
        private IBankAccountService bankAccountService;
        private IPaymentService paymentService;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PaymentController(IBankAccountService bankAccountService, IPaymentService paymentService)
        {
            this.bankAccountService = bankAccountService;
            this.paymentService = paymentService;
        }

        private IUserService UserService
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<IUserService>();
            }
        }

        // Осуществленные платежи
        [LoggedExceptionFilter]
        public async Task<ActionResult> PaymentData(int? id, string sortOrder)
        {
            logger.Info("Клиент запросил информацию об осуществленных платежах банковского счета с id {0}", id);

            ViewBag.BankAccountId = id;
            var paymentsBankAccountDTO = await paymentService.PaymentData(id);
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<PaymentBankAccountDTO,
                PaymentBankAccount>()).CreateMapper();
            var paymentsCurrentBankAccount = mapper.Map<IEnumerable<PaymentBankAccountDTO>,
                IEnumerable<PaymentBankAccount>>(paymentsBankAccountDTO);
            switch (sortOrder)
            {
                case "Number":
                    paymentsCurrentBankAccount = paymentsCurrentBankAccount
                        .OrderBy(payment => payment.Id).ToList();
                    break;
                case "Number_desc":
                    paymentsCurrentBankAccount = paymentsCurrentBankAccount
                        .OrderByDescending(payment => payment.Id).ToList();
                    break;
                case "Date":
                    paymentsCurrentBankAccount = paymentsCurrentBankAccount
                        .OrderBy(payment => payment.DateTime).ToList();
                    break;
                case "Date_desc":
                    paymentsCurrentBankAccount = paymentsCurrentBankAccount
                        .OrderByDescending(payment => payment.DateTime).ToList();
                    break;
            }
            return View(paymentsCurrentBankAccount);
        }

        // Подготовленные платежи
        public async Task<ActionResult> PreparedPaymentsData()
        {
            string nameCurrentUser = User.Identity.Name;
            string userId = await UserService.FindUserIdAsync(nameCurrentUser);

            logger.Info("Клиент {0} запросил информацию об подготовленных платежах", userId);

            IEnumerable<int> bankAccountsIds = await bankAccountService
                .FindBankAccoutsForPreparedPayments(userId);
            var preparedPaymentsBankAccountDTO = await paymentService.FindPreparedPayments(bankAccountsIds);

            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<PreparedPaymentDTO,
                PreparedPayment>()).CreateMapper();
            var preparedPaymentsBankAccount = mapper.Map<IEnumerable<PreparedPaymentDTO>,
                IEnumerable<PreparedPayment>>(preparedPaymentsBankAccountDTO);
            return View(preparedPaymentsBankAccount);
        }

        // Подготовить платеж
        [HttpGet]
        public ActionResult PreparePayment(int id)
        {
            PreparationPaymentModel model = new PreparationPaymentModel
            {
                BankAccountId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PreparePayment(PreparationPaymentModel model)
        {
            if (ModelState.IsValid)
            {
                var mapper = new MapperConfiguration(cfg => cfg.CreateMap<PreparationPaymentModel,
                PreparationPaymentDTO>()).CreateMapper();
                var paymentsCurrentBankAccount = mapper.Map<PreparationPaymentModel,
                     PreparationPaymentDTO>(model);
                await paymentService.PreparePayment(paymentsCurrentBankAccount);

                logger.Info("Клиент подготовил платеж для банковского счета {0}", model.BankAccountId);

                return RedirectToAction("PreparedPaymentsData", "Payment");
            }
            return View(model);
        }

        // Пополнить один счет другим счетом
        [Authorize(Roles = "client")]
        [HttpGet]
        public ActionResult ReplenishBankAccount(int id)
        {
            ReplenishBankAccountModel replenishBankAccountModel = new ReplenishBankAccountModel();
            replenishBankAccountModel.BankAccountId = id;
            return View(replenishBankAccountModel);
        }

        [HttpPost]
        public async Task<ActionResult> ReplenishBankAccount(ReplenishBankAccountModel model)
        {
            try
            {
                await bankAccountService.CheckBankAccount(model.NumberBankAccount);
            }
            catch(ValidationBusinessLogicException exception)
            {
                ModelState.AddModelError(exception.Property, exception.Message);
            }
            if (ModelState.IsValid)
            {
                var mapper = new MapperConfiguration(cfg => cfg.CreateMap<ReplenishBankAccountModel,
               ReplenishBankAccountDTO>()).CreateMapper();
                var paymentsCurrentBankAccount = mapper.Map<ReplenishBankAccountModel,
                     ReplenishBankAccountDTO>(model);
                await paymentService.ReplenishBankAccount(paymentsCurrentBankAccount);

                logger.Info("Клиент подготовил пополнение с помощью банковского счета id = {0} " +
                    "банковский счет с NumberBankAccount = {1}", model.BankAccountId, model.NumberBankAccount);

                return RedirectToAction("PreparedPaymentsData");
            }
            return View(model);
        }

        // Подтвердить платеж
        [HttpPost]
        public async Task<ActionResult> Pay(List<PreparedPayment> model)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<PreparedPayment,
               PreparedPaymentDTO>()).CreateMapper();
            var paymentsCurrentBankAccount = mapper.Map<IEnumerable<PreparedPayment>,
                IEnumerable<PreparedPaymentDTO>>(model);

            await paymentService.Pay(paymentsCurrentBankAccount);

            string userName = User.Identity.Name;
            string userId = await UserService.FindUserIdAsync(userName);

            logger.Info("Клиент {0} подтвердил платежи", userId);

            return RedirectToAction("PreparedPaymentsData", "Payment");
        }

        // Отклонить платеж
        [LoggedExceptionFilter]
        public async Task<ActionResult> RejectPayment(int? id)
        {
            await paymentService.RejectPayment(id);

            logger.Info("Клиент отклонил платеж {0}", id);

            return RedirectToAction("PreparedPaymentsData", "Payment");
        }
    }
}