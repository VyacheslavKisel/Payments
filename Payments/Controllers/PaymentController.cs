using Payments.ViewModels;
using Payments.ViewModels.Payment;
using Service;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Payments.Controllers
{
    [Authorize(Roles = "client")]
    public class PaymentController : Controller
    {
        ApplicationContext _db = new ApplicationContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PaymentData(int id)
        {
            ViewBag.BankAccountId = id;
            List<Payment> payments = _db.Payments.ToList();
            var paymentsCurrentBankAccount = payments
                .Where(payment => payment.BankAccountId == id)
                .Select(payment => new PaymentBankAccount(payment.Id, payment.DateTime, payment.Status,
                payment.Sum, payment.Recipient, payment.CodeEgrpou, payment.CodeIban, payment.Purpose))
                .ToList();
            return View(paymentsCurrentBankAccount);
        }

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
                // Todo Редиректит на главную страницу счетов - переделать
                return RedirectToAction("BankAccountsData", "BankAccount");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult MakePayment(int id)
        {
            // Todo - обратить внимание на запросы
            Payment payment = _db.Payments.Find(id);
            payment.Status = "отправленный";
            payment.DateTime = DateTime.Now;
            BankAccount bankAccount = _db.BankAccounts.FirstOrDefault(p => p.Id == payment.BankAccountId);
            bankAccount.Balance -= payment.Sum;
            _db.Entry(payment).State = EntityState.Modified;
            _db.Entry(bankAccount).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("BankAccountsData", "BankAccount");
        }
    }
}