using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.Payment
{
    public class PaymentBankAccount
    {
        public PaymentBankAccount(int id, DateTime dateTime, string status, double sum,
            string recipient, string codeEgrpou, string codeIban, string purpose)
        {
            Id = id;
            DateTime = dateTime;
            Status = status;
            Sum = sum;
            Recipient = recipient;
            CodeEgrpou = codeEgrpou;
            CodeIban = codeIban;
            Purpose = purpose;
        }
        public int Id { get; }
        public DateTime DateTime { get; }
        public string Status { get; }
        public double Sum { get; }
        public string Recipient { get; }
        public string CodeEgrpou { get; }
        public string CodeIban { get; }
        public string Purpose { get; }
    }
}