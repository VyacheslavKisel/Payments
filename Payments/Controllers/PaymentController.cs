using Microsoft.AspNet.Identity.Owin;
using Payments.ViewModels;
using Payments.ViewModels.BankAccount;
using Payments.ViewModels.Payment;
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

        ApplicationContext _db = new ApplicationContext();

        public ActionResult Index()
        {
            return View();
        }

        // Осуществленные платежи
        public ActionResult PaymentData(int id)
        {
            ViewBag.BankAccountId = id;
            List<Payment> payments = _db.Payments.ToList();
            var paymentsCurrentBankAccount = payments
                .Where(payment => payment.BankAccountId == id && (payment.Status == "отправленный" || payment.Status == "отклоненный"))
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
            // дублирование кода из BankAccountController - BankAccountsData
            List<BankAccount> bankAccountsAll = _db.BankAccounts.ToList();
            var bankAccountsUser = bankAccountsAll
                .Where(bankAccount => bankAccount.ApplicationUserId == currentUser.Id && bankAccount.LockoutEnabled == false)
                .Select(bankAccount => new BankAccountUser(bankAccount.Id, bankAccount.NumberCard,
                bankAccount.Name, bankAccount.Balance))
                .ToList();
            // слишком много кода, надо сделать проще запрос
            List<Payment> payments = _db.Payments.ToList();
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
        public ActionResult PreparePayment(PreparationPaymentModel model)
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
                _db.Payments.Add(payment);
                _db.SaveChanges();
                return RedirectToAction("PreparedPaymentsData", "Payment");
            }
            return View(model);
        }

        // Подтвердить платеж
        [HttpPost]
        public ActionResult Pay(List<PreparedPayment> model)
        {
            foreach (var item in model)
            {
                Payment payment = _db.Payments.Find(item.Id);
                BankAccount bankAccount = _db.BankAccounts.FirstOrDefault(p => p.Id == payment.BankAccountId);
                if(bankAccount.Balance >= payment.Sum)
                {
                    payment.Status = "отправленный";
                    payment.DateTime = DateTime.Now;
                    bankAccount.Balance -= payment.Sum;
                    _db.Entry(payment).State = EntityState.Modified;
                    _db.Entry(bankAccount).State = EntityState.Modified;
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }

        // Отклонить платеж
        public ActionResult RejectPayment(int id)
        {
            Payment payment = _db.Payments.Find(id);
            payment.Status = "отклоненный";
            payment.DateTime = DateTime.Now;
            _db.Entry(payment).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("PreparedPaymentsData", "Payment");
        }
    }
}