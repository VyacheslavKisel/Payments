using Payments.BLL.BusinessModels;
using Payments.BLL.DTO.BankAccount;
using Payments.BLL.Infrastructure;
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
    // Сервис по работе с банковскими счетами
    public class BankAccountService : IBankAccountService
    {
        private IUnitOfWork database;

        public BankAccountService(IUnitOfWork unitOfWork)
        {
            database = unitOfWork;
        }

        public async Task CreateBankAccount(CreatureBankAccountDTO creatureBankAccountDTO)
        {
            BankAccount existingNumberBankAccount;
            string formedNumberBankAccount;
            do
            {
                existingNumberBankAccount = null;
                formedNumberBankAccount = MethodsBankAccount.FormNumberAccount();
                existingNumberBankAccount = await database.BankAccounts
                    .FindAsync(p => p.NumberAccount == formedNumberBankAccount);
            }
            while (existingNumberBankAccount != null);
            BankAccount bankAccount = new BankAccount()
            {
                NumberCard = creatureBankAccountDTO.NumberCard,
                Name = creatureBankAccountDTO.Name,
                Balance = 20000,
                ApplicationUserId = creatureBankAccountDTO.ApplicationUserId,
                LockoutEnabled = false,
                RequestUnblock = false,
                NumberAccount = formedNumberBankAccount
            };
            database.BankAccounts.Create(bankAccount);
            await database.SaveAsync();
        }

        public async Task<IEnumerable<BankAccountUserDTO>> BankAccountsData(string currentUserId)
        {
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts.
                FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUserId &&
                bankAccount.LockoutEnabled == false);
            var bankAccountsUser = bankAccountsAll
                .Select(bankAccount => new BankAccountUserDTO(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard,
                bankAccount.Name, bankAccount.Balance))
                .ToList();
            return bankAccountsUser;
        }

        public async Task<IEnumerable<BankAccountDataForAdminDTO>> BankAccountsDataForAdmin(string id)
        {
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
                .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == id);
            var bankAccountsUser = bankAccountsAll
                .Select(bankAccount => new BankAccountDataForAdminDTO(bankAccount.Id, bankAccount.NumberAccount,
                bankAccount.NumberCard, bankAccount.Name, bankAccount.Balance,
                bankAccount.LockoutEnabled, bankAccount.RequestUnblock))
                .ToList();
            return bankAccountsUser;
        }

        public async Task BlockBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception();
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    bankAccount.LockoutEnabled = true;
                    database.BankAccounts.Update(bankAccount);
                    await database.SaveAsync();
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        public async Task UnBlockBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception();
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    bankAccount.LockoutEnabled = false;
                    bankAccount.RequestUnblock = false;
                    database.BankAccounts.Update(bankAccount);
                    await database.SaveAsync();
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        public async Task BlockSelfBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception();
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    bankAccount.LockoutEnabled = true;
                    database.BankAccounts.Update(bankAccount);
                    await database.SaveAsync();
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        public async Task RequestUnblockBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception();
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    bankAccount.RequestUnblock = true;
                    database.BankAccounts.Update(bankAccount);
                    await database.SaveAsync();
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        public async Task CheckBankAccount(string numberBankAccount)
        {
            BankAccount bankAccount = await database.BankAccounts
                    .FindAsync(b => b.NumberAccount == numberBankAccount);
            if(bankAccount == null)
            {
                throw new ValidationBusinessLogicException("Нет номера счета в текущем банке", "NumberBankAccount");
            }
        }

        public async Task<IEnumerable<int>> FindBankAccoutsForPreparedPayments(string applicationUserId)
        {
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
                .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == applicationUserId 
                && bankAccount.LockoutEnabled == false);
            List<int> bankAccountsIds = new List<int>();
            foreach (var item in bankAccountsAll)
            {
                bankAccountsIds.Add(item.Id);
            }
            return bankAccountsIds;
        }

        public async Task<IEnumerable<BankAccountSecurityDTO>> GetAllBankAccountsUserAsync(string currentUserId)
        {
            ApplicationUser currentUser = await database.UserManager.FindByNameAsync(currentUserId);
            IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts.
                FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUser.Id);
            var bankAccountsUser = bankAccountsAll
                .Select(bankAccount => new BankAccountSecurityDTO(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard,
                bankAccount.Name, bankAccount.Balance, bankAccount.LockoutEnabled, bankAccount.RequestUnblock))
                .ToList();
            return bankAccountsUser;
        }
    }
}
