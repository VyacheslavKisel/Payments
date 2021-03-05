using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.Payment
{
    public class ReplenishBankAccountDTO
    {
        public int BankAccountId { get; set; }
        public string NumberBankAccount { get; set; }
        public string Sum { get; set; }
    }
}
