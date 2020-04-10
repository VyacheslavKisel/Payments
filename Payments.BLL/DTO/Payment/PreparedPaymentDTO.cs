using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.Payment
{
    // Данные о подготовленных платежах
    public class PreparedPaymentDTO
    {
        public PreparedPaymentDTO(int id, string numberAccount, DateTime dateTime, string status, double sum,
            string recipient, string codeEgrpou, string codeIban, string purpose)
        {
            Id = id;
            NumberAccount = numberAccount;
            DateTime = dateTime;
            Status = status;
            Sum = sum;
            Recipient = recipient;
            CodeEgrpou = codeEgrpou;
            CodeIban = codeIban;
            Purpose = purpose;
        }
        public PreparedPaymentDTO() { }
        public int Id { get; set; }
        public string NumberAccount { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public double Sum { get; set; }
        public string Recipient { get; set; }
        public string CodeEgrpou { get; set; }
        public string CodeIban { get; set; }
        public string Purpose { get; set; }
    }
}
