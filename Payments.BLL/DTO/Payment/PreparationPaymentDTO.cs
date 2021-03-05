using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.Payment
{
    public class PreparationPaymentDTO
    {
        public int BankAccountId { get; set; }
        public string Sum { get; set; }
        public string Recipient { get; set; }
        public string CodeEgrpou { get; set; }
        public string CodeIban { get; set; }
        public string Purpose { get; set; }
    }
}
