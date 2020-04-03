using Microsoft.AspNet.Identity.Owin;
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

        //ApplicationContext _db = new ApplicationContext();
        private UnitOfWork database;

        public PaymentController()
        {
            database = new UnitOfWork();
        }

        // Осуществленные платежи
        public async Task<ActionResult> PaymentData(int id)
        {
            ViewBag.BankAccountId = id;
            IEnumerable<Payment> payments = await database.Payments
                .FindAllAsync(payment => payment.BankAccountId == id && 
                (payment.Status == "отправленный" || payment.Status == "отклоненный"));
            var paymentsCurrentBankAccount = payments
                .Select(payment => new PaymentBankAccount(payment.Id, payment.DateTime, payment.Status,
                payment.Sum, payment.Recipient, payment.CodeEgrpou, payment.CodeIban, payment.Purpose))
                .ToList();
            return View(paymentsCurrentBankAccount);
        }

        // Подготовленные платежи
        public async Task<ActionResult> PreparedPaymentsData()
        {
            string nameCurrentUser = User.Identity.Name;
            ApplicationUser currentUser = await UserManager.FindByNameAsync(nameCurrentUser);
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
                .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUser.Id && bankAccount.LockoutEnabled == false);
            var bankAccountsUser = bankAccountsAll
                .Select(bankAccount => new BankAccountUser(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard,
                bankAccount.Name, bankAccount.Balance))
                .ToList();
            IEnumerable<Payment> payments = await database.Payments.GetAllAsync();
            List<PreparedPayment> preparedPaymentsBankAccount = new List<PreparedPayment>();
            foreach (var item in payments)
            {
                foreach (var itemBankAccount in bankAccountsUser)
                {
                    if(item.BankAccountId == itemBankAccount.Id && item.Status == "подготовленный")
                    {
                        PreparedPayment preparedPayment = new PreparedPayment(item.Id,
                            item.DateTime, item.Status, item.Sum, item.Recipient, item.CodeEgrpou,
                            item.CodeIban, item.Purpose);
                        preparedPaymentsBankAccount.Add(preparedPayment);
                    }
                }
            }
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
            if(ModelState.IsValid)
            {
                Payment payment = new Payment()
                {
                    Sum = Convert.ToDouble(model.Sum),
                    Purpose = model.Purpose,
                    Recipient = model.Recipient,
                    CodeEgrpou = model.CodeEgrpou,
                    CodeIban = model.CodeIban,
                    BankAccountId = model.BankAccountId,
                    DateTime = DateTime.Now,
                    Status = "подготовленный"
                };
                database.Payments.Create(payment);
                await database.SaveAsync();
                return RedirectToAction("PreparedPaymentsData", "Payment");
            }
            return View(model);
        }

        // Подтвердить платеж
        [HttpPost]
        public async Task<ActionResult> Pay(List<PreparedPayment> model)
        {
            foreach (var item in model)
            {
                Payment payment = await database.Payments.GetAsync(item.Id);
                BankAccount bankAccount = await database.BankAccounts.FindAsync(p => p.Id == payment.BankAccountId);
                if(bankAccount.Balance >= payment.Sum)
                {
                    payment.Status = "отправленный";
                    payment.DateTime = DateTime.Now;
                    bankAccount.Balance -= payment.Sum;
                    database.Payments.Update(payment);
                    database.BankAccounts.Update(bankAccount);
                    await database.SaveAsync();
                }
            }
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }

        // Отклонить платеж
        public async Task<ActionResult> RejectPayment(int id)
        {
            Payment payment = await database.Payments.GetAsync(id);
            payment.Status = "отклоненный";
            payment.DateTime = DateTime.Now;
            database.Payments.Update(payment);
            await database.SaveAsync();
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }
    }
}