using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.Payment
{
    public class PreparedPayment
    {
        public PreparedPayment(int id, DateTime dateTime, string status, double sum,
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
        public PreparedPayment() { }
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public double Sum { get; set; }
        public string Recipient { get; set; }
        public string CodeEgrpou { get; set; }
        public string CodeIban { get; set; }
        public string Purpose { get; set; }
    }
}