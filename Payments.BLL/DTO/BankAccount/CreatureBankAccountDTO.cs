using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.BankAccount
{
    public class CreatureBankAccountDTO
    {
        public CreatureBankAccountDTO(string applicationUserId, string numberCard, string name)
        {
            ApplicationUserId = applicationUserId;
            NumberCard = numberCard;
            Name = name;
        }
        public string ApplicationUserId { get; set; }
        public string NumberCard { get; set; }
        public string Name { get; set; }
    }
}
