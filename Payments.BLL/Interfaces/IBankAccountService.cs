using Payments.BLL.DTO.BankAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Interfaces
{
    public interface IBankAccountService
    {
        Task CreateBankAccount(CreatureBankAccountDTO creatureBankAccountDTO);
        Task<IEnumerable<BankAccountUserDTO>> BankAccountsData(string currentUserId);
        Task<IEnumerable<BankAccountDataForAdminDTO>> BankAccountsDataForAdmin(string id);
        Task BlockBankAccount(int? id);
        Task UnBlockBankAccount(int? id);
        Task BlockSelfBankAccount(int? id);
        Task RequestUnblockBankAccount(int? id);
        Task CheckBankAccount(string numberBankAccount);
        Task<IEnumerable<int>> FindBankAccoutsForPreparedPayments(string applicationUserId);
        Task<IEnumerable<BankAccountSecurityDTO>> GetAllBankAccountsUserAsync(string currentUserId);
    }
}
