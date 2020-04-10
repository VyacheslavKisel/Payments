using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Payments.BLL.DTO.Payment;
using Payments.BLL.Infrastructure;
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
        private BankAccountService bankAccountService;

        private PaymentService paymentService;

        public PaymentController()
        {
            bankAccountService = new BankAccountService();
            paymentService = new PaymentService();
        }

        private UserService UserService
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<UserService>();
            }
        }

        // Осуществленные платежи
        public async Task<ActionResult> PaymentData(int id, string sortOrder)
        {
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
            string idCurrentUser = await UserService.FindUserIdAsync(nameCurrentUser);
            IEnumerable<int> bankAccountsIds = await bankAccountService
                .FindBankAccoutsForPreparedPayments(idCurrentUser);
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
            ViewBag.BankAccountId = id;
            return View();
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
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }

        // Отклонить платеж
        [LoggedExceptionFilter]
        public async Task<ActionResult> RejectPayment(int? id)
        {
            await paymentService.RejectPayment(id);
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }
    }
}