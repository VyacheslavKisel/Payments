using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Payments.BLL.DTO.Payment;
using Payments.BLL.Infrastructure;
using Payments.BLL.Services;
using Payments.Filters;
using Payments.ViewModels;
using Payments.ViewModels.BankAccount;
using Payments.ViewModels.Payment;
using Service;
using Service.Models;
using Service.Repositories;
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
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private UnitOfWork database;

        public PaymentController()
        {
            database = new UnitOfWork();
        }

        PaymentService paymentService = new PaymentService();
        BankAccountService bankAccountService = new BankAccountService();

        // Осуществленные платежи
        public async Task<ActionResult> PaymentData(int id, string sortOrder)
        {
            ViewBag.BankAccountId = id;
            //IEnumerable<Payment> payments = await database.Payments
            //    .FindAllAsync(payment => payment.BankAccountId == id &&
            //    (payment.Status == "отправленный"));
            //var paymentsCurrentBankAccount = payments
            //    .Select(payment => new PaymentBankAccount(payment.Id, payment.DateTime, payment.Status,
            //    payment.Sum, payment.Recipient, payment.CodeEgrpou, payment.CodeIban, payment.Purpose))
            //    .ToList();
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
            ApplicationUser currentUser = await UserManager.FindByNameAsync(nameCurrentUser);
            //IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
            //    .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUser.Id && bankAccount.LockoutEnabled == false);
            //var bankAccountsUser = bankAccountsAll
            //    .Select(bankAccount => new BankAccountUser(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard,
            //    bankAccount.Name, bankAccount.Balance))
            //    .ToList();
            IEnumerable<int> bankAccountsIds = await bankAccountService
                .FindBankAccoutsForPreparedPayments(currentUser.Id);
            //IEnumerable<Payment> payments = await database.Payments.GetAllIncludeLinkedDataAsync();
            //List<PreparedPayment> preparedPaymentsBankAccount = new List<PreparedPayment>();
            //foreach (var item in payments)
            //{
            //    foreach (var itemBankAccount in bankAccountsUser)
            //    {
            //        if (item.BankAccountId == itemBankAccount.Id && item.Status == "подготовленный")
            //        {
            //            PreparedPayment preparedPayment = new PreparedPayment(item.Id, item.BankAccount.NumberAccount,
            //                item.DateTime, item.Status, item.Sum, item.Recipient, item.CodeEgrpou,
            //                item.CodeIban, item.Purpose);
            //            preparedPaymentsBankAccount.Add(preparedPayment);
            //        }
            //    }
            //}
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
                //Payment payment = new Payment()
                //{
                //    Sum = Convert.ToDouble(model.Sum),
                //    Purpose = model.Purpose,
                //    Recipient = model.Recipient,
                //    CodeEgrpou = model.CodeEgrpou,
                //    CodeIban = model.CodeIban,
                //    BankAccountId = model.BankAccountId,
                //    DateTime = DateTime.Now,
                //    Status = "подготовленный"
                //};
                //database.Payments.Create(payment);
                //await database.SaveAsync();
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
            //BankAccount bankAccount = await database.BankAccounts
            //        .FindAsync(b => b.NumberAccount == model.NumberBankAccount);
            //if (bankAccount == null)
            //{
            //    ModelState.AddModelError("NumberBankAccount", "Нет номера счета в текущем банке");
            //}
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
                //Payment payment = new Payment()
                //{
                //    Sum = Convert.ToDouble(model.Sum),
                //    BankAccountId = model.BankAccountId,
                //    DateTime = DateTime.Now,
                //    Status = "подготовленный",
                //    Purpose = "Пополнение банковского счета",
                //    Recipient = "Текущий банк",
                //    CodeEgrpou = "55558888",
                //    CodeIban = "UA" + "12" + "555789" + "00000" + model.NumberBankAccount
                //};
                //database.Payments.Create(payment);
                //await database.SaveAsync();
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
            //foreach (var item in model)
            //{
            //    Payment payment = await database.Payments.GetAsync(item.Id);
            //    BankAccount bankAccount = await database.BankAccounts.FindAsync(b => b.Id == payment.BankAccountId);
            //    if (bankAccount.Balance >= payment.Sum)
            //    {
            //        payment.Status = "отправленный";
            //        payment.DateTime = DateTime.Now;
            //        bankAccount.Balance -= payment.Sum;
            //        database.Payments.Update(payment);
            //        database.BankAccounts.Update(bankAccount);
            //        if (payment.CodeIban.Substring(0, 2) == "UA"
            //       && payment.CodeIban.Substring(4, 6) == "555789")
            //        {
            //            string numberBankAccountReceiver = payment.CodeIban.Substring(15);
            //            BankAccount bankAccountReceiver = await database.BankAccounts
            //                .FindAsync(b => b.NumberAccount == numberBankAccountReceiver);
            //            bankAccountReceiver.Balance += payment.Sum;
            //            database.BankAccounts.Update(bankAccountReceiver);
            //        }
            //        await database.SaveAsync();
            //    }
            //}
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }

        // Отклонить платеж
        [LoggedExceptionFilter]
        public async Task<ActionResult> RejectPayment(int? id)
        {
            await paymentService.RejectPayment(id);
            //Payment payment = await database.Payments.GetAsync(id);
            //payment.Status = "отклоненный";
            //payment.DateTime = DateTime.Now;
            //database.Payments.Update(payment);
            //await database.SaveAsync();
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }
    }
}