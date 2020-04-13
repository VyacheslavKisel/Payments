using NLog;
using Payments.BLL.DTO.Payment;
using Payments.BLL.Interfaces;
using Service.Interfaces;
using Service.Models;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Services
{
    // Сервис по работе с платежами
    public class PaymentService : IPaymentService
    {
        private IUnitOfWork database;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PaymentService(IUnitOfWork unitOfWork)
        {
            database = unitOfWork;
        }

        public async Task<IEnumerable<PaymentBankAccountDTO>> PaymentData(int? id)
        {
            IEnumerable<PaymentBankAccountDTO> paymentsCurrentBankAccount = null;
            if (id == null || await database.BankAccounts.GetAsync((int)id) == null)
            {
                throw new Exception("Не возможно найти платежи, " +
                    "так как не существует банковского счета с запрашиваемым id");
            }
            else
            {
                try
                {
                    IEnumerable<Payment> payments = await database.Payments
                    .FindAllAsync(payment => payment.BankAccountId == id &&
                    (payment.Status == "отправленный"));
                    paymentsCurrentBankAccount = payments
                        .Select(payment => new PaymentBankAccountDTO(payment.Id, payment.DateTime, payment.Status,
                        payment.Sum, payment.Recipient, payment.CodeEgrpou, payment.CodeIban, payment.Purpose))
                        .ToList();
                }
                catch (Exception exception)
                {
                    logger.Error($"{exception.Message} {exception.StackTrace}");
                }
            }
            
            return paymentsCurrentBankAccount;
        }

        public async Task PreparePayment(PreparationPaymentDTO preparationPaymentDTO)
        {      
            try
            {
                Payment payment = new Payment()
                {
                    Sum = Convert.ToDouble(preparationPaymentDTO.Sum),
                    Purpose = preparationPaymentDTO.Purpose,
                    Recipient = preparationPaymentDTO.Recipient,
                    CodeEgrpou = preparationPaymentDTO.CodeEgrpou,
                    CodeIban = preparationPaymentDTO.CodeIban,
                    BankAccountId = preparationPaymentDTO.BankAccountId,
                    DateTime = DateTime.Now,
                    Status = "подготовленный"
                };
                database.Payments.Create(payment);
                await database.SaveAsync();
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
        }

        public async Task<IEnumerable<PreparedPaymentDTO>> FindPreparedPayments(IEnumerable<int> bankAccountsIds)
        {
            List<PreparedPaymentDTO> preparedPaymentsBankAccount = new List<PreparedPaymentDTO>();           
            try
            {
                IEnumerable<Payment> payments = await database.Payments.GetAllIncludeLinkedDataAsync();
                foreach (var item in payments)
                {
                    foreach (var itemBankAccount in bankAccountsIds)
                    {
                        if (item.BankAccountId == itemBankAccount && item.Status == "подготовленный")
                        {
                            PreparedPaymentDTO preparedPayment = new PreparedPaymentDTO(item.Id, item.BankAccount.NumberAccount,
                                item.DateTime, item.Status, item.Sum, item.Recipient, item.CodeEgrpou,
                                item.CodeIban, item.Purpose);
                            preparedPaymentsBankAccount.Add(preparedPayment);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return preparedPaymentsBankAccount;
        }

        public async Task ReplenishBankAccount(ReplenishBankAccountDTO replenishBankAccountDTO)
        {          
            try
            {
                Payment payment = new Payment()
                {
                    Sum = Convert.ToDouble(replenishBankAccountDTO.Sum),
                    BankAccountId = replenishBankAccountDTO.BankAccountId,
                    DateTime = DateTime.Now,
                    Status = "подготовленный",
                    Purpose = "Пополнение банковского счета",
                    Recipient = "Текущий банк",
                    CodeEgrpou = "55558888",
                    CodeIban = "UA" + "12" + "555789" + "00000" + replenishBankAccountDTO.NumberBankAccount
                };
                database.Payments.Create(payment);
                await database.SaveAsync();
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
        }

        public async Task Pay(IEnumerable<PreparedPaymentDTO> preparedPaymentDTO)
        {     
            try
            {
                foreach (var item in preparedPaymentDTO)
                {
                    Payment payment = await database.Payments.GetAsync(item.Id);
                    BankAccount bankAccount = await database.BankAccounts.FindAsync(b => b.Id == payment.BankAccountId);
                    if (bankAccount.Balance >= payment.Sum)
                    {
                        payment.Status = "отправленный";
                        payment.DateTime = DateTime.Now;
                        bankAccount.Balance -= payment.Sum;
                        database.Payments.Update(payment);
                        database.BankAccounts.Update(bankAccount);
                        if (payment.CodeIban.Substring(0, 2) == "UA"
                       && payment.CodeIban.Substring(4, 6) == "555789")
                        {
                            string numberBankAccountReceiver = payment.CodeIban.Substring(15);
                            BankAccount bankAccountReceiver = await database.BankAccounts
                                .FindAsync(b => b.NumberAccount == numberBankAccountReceiver);
                            bankAccountReceiver.Balance += payment.Sum;
                            database.BankAccounts.Update(bankAccountReceiver);
                        }
                        await database.SaveAsync();
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
        }

        public async Task RejectPayment(int? id)
        {
            if (id == null)
            {
                throw new Exception("Не возможно отклонить платеж с запрашиваемым id");
            }
            else
            {
                Payment payment = await database.Payments.GetAsync((int)id);
                if (payment != null)
                { 
                    try
                    {
                        payment.Status = "отклоненный";
                        payment.DateTime = DateTime.Now;
                        database.Payments.Update(payment);
                        await database.SaveAsync();
                    }
                    catch (Exception exception)
                    {
                        logger.Error($"{exception.Message} {exception.StackTrace}");
                    }
                }
                else
                {
                    throw new Exception("Не возможно отклонить платеж с запрашиваемым id");
                }
            }
        }
    }
}
