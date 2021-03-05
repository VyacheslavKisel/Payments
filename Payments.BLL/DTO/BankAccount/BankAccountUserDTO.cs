using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.BankAccount
{
    public class BankAccountUserDTO
    {
        public BankAccountUserDTO(int id, string numberAccount, string numberCard, string name, double balance)
        {
            Id = id;
            NumberAccount = numberAccount;
            NumberCard = numberCard;
            Name = name;
            Balance = balance;
        }
        public int Id { get; }
        public string NumberAccount { get; }
        public string NumberCard { get; }
        public string Name { get; }
        public double Balance { get; }
    }
}
