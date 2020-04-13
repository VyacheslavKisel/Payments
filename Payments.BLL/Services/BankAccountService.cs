using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BankAccountService(IUnitOfWork unitOfWork)
        {
            database = unitOfWork;
        }

        public async Task CreateBankAccount(CreatureBankAccountDTO creatureBankAccountDTO)
        {
            try
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
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
        }

        public async Task<IEnumerable<BankAccountUserDTO>> BankAccountsData(string currentUserId)
        {
            IEnumerable<BankAccountUserDTO> bankAccountsUser = null;
            if (currentUserId == null || await database.UserManager.FindByIdAsync(currentUserId) == null)
            {
                throw new Exception("Не возможно найти банковские счета, так как не существует пользователя с запрашиваемым id");
            }
            else
            {
                try
                {
                    IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts.
                    FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUserId &&
                    bankAccount.LockoutEnabled == false);
                    bankAccountsUser = bankAccountsAll
                        .Select(bankAccount => new BankAccountUserDTO(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard,
                        bankAccount.Name, bankAccount.Balance))
                        .ToList();
                }
                catch (Exception exception)
                {
                    logger.Error($"{exception.Message} {exception.StackTrace}");
                }
            }
           
            return bankAccountsUser;
        }

        public async Task<IEnumerable<BankAccountDataForAdminDTO>> BankAccountsDataForAdmin(string id)
        {
            IEnumerable<BankAccountDataForAdminDTO> bankAccountsUser = null;
            if (id == null || await database.UserManager.FindByIdAsync(id) == null)
            {
                throw new Exception("Не возможно найти банковские счета, так как не существует пользователя с запрашиваемым id");
            }
            else
            {
                try
                {
                    IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
                    .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == id);
                    bankAccountsUser = bankAccountsAll
                        .Select(bankAccount => new BankAccountDataForAdminDTO(bankAccount.Id, bankAccount.NumberAccount,
                        bankAccount.NumberCard, bankAccount.Name, bankAccount.Balance,
                        bankAccount.LockoutEnabled, bankAccount.RequestUnblock))
                        .ToList();
                }
                catch (Exception exception)
                {
                    logger.Error($"{exception.Message} {exception.StackTrace}");
                }
            }         
            return bankAccountsUser;
        }

        public async Task BlockBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception("Не существует банковского счета с запрашиваемым id");
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    try
                    {
                        bankAccount.LockoutEnabled = true;
                        database.BankAccounts.Update(bankAccount);
                        await database.SaveAsync();
                    }
                    catch (Exception exception)
                    {
                        logger.Error($"{exception.Message} {exception.StackTrace}");
                    }
                }
                else
                {
                    throw new Exception("Не существует банковского счета с запрашиваемым id");
                }
            }
        }

        public async Task UnBlockBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception("Не существует банковского счета с запрашиваемым id");
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    try
                    {
                        bankAccount.LockoutEnabled = false;
                        bankAccount.RequestUnblock = false;
                        database.BankAccounts.Update(bankAccount);
                        await database.SaveAsync();
                    }
                    catch (Exception exception)
                    {
                        logger.Error($"{exception.Message} {exception.StackTrace}");
                    }
                }
                else
                {
                    throw new Exception("Не существует банковского счета с запрашиваемым id");
                }
            }
        }

        public async Task BlockSelfBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception("Не существует банковского счета с запрашиваемым id");
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    try
                    {
                        bankAccount.LockoutEnabled = true;
                        database.BankAccounts.Update(bankAccount);
                        await database.SaveAsync();
                    }
                    catch (Exception exception)
                    {
                        logger.Error($"{exception.Message} {exception.StackTrace}");
                    }
                }
                else
                {
                    throw new Exception("Не существует банковского счета с запрашиваемым id");
                }
            }
        }

        public async Task RequestUnblockBankAccount(int? id)
        {
            if (id == null)
            {
                throw new Exception("Не существует банковского счета с запрашиваемым id");
            }
            else
            {
                BankAccount bankAccount = await database.BankAccounts.GetAsync((int)id);
                if (bankAccount != null)
                {
                    try
                    {
                        bankAccount.RequestUnblock = true;
                        database.BankAccounts.Update(bankAccount);
                        await database.SaveAsync();
                    }
                    catch (Exception exception)
                    {
                        logger.Error($"{exception.Message} {exception.StackTrace}");
                    }
                }
                else
                {
                    throw new Exception("Не существует банковского счета с запрашиваемым id");
                }
            }
        }

        public async Task CheckBankAccount(string numberBankAccount)
        {
            BankAccount bankAccount = null;
            try
            {
                bankAccount = await database.BankAccounts
                    .FindAsync(b => b.NumberAccount == numberBankAccount);
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            if (bankAccount == null)
            {
                throw new ValidationBusinessLogicException("Нет номера счета в текущем банке", "NumberBankAccount");
            }
        }

        public async Task<IEnumerable<int>> FindBankAccoutsForPreparedPayments(string applicationUserId)
        {
            List<int> bankAccountsIds = new List<int>();
            try
            {
                IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts
               .FindAllAsync(bankAccount => bankAccount.ApplicationUserId == applicationUserId
               && bankAccount.LockoutEnabled == false);
                foreach (var item in bankAccountsAll)
                {
                    bankAccountsIds.Add(item.Id);
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return bankAccountsIds;
        }

        public async Task<IEnumerable<BankAccountSecurityDTO>> GetAllBankAccountsUserAsync(string currentUserId)
        {
            IEnumerable<BankAccountSecurityDTO> bankAccountsUser = null;
            if (currentUserId == null || await database.UserManager.FindByIdAsync(currentUserId) == null)
            {
                throw new Exception("Не возможно найти банковские счета, так как не существует пользователя с запрашиваемым id");
            }
            else
            {
                try
                {
                    ApplicationUser currentUser = await database.UserManager.FindByIdAsync(currentUserId);
                    IEnumerable<BankAccount> bankAccountsAll = await database.BankAccounts.
                        FindAllAsync(bankAccount => bankAccount.ApplicationUserId == currentUser.Id);
                    bankAccountsUser = bankAccountsAll
                        .Select(bankAccount => new BankAccountSecurityDTO(bankAccount.Id, bankAccount.NumberAccount, bankAccount.NumberCard,
                        bankAccount.Name, bankAccount.Balance, bankAccount.LockoutEnabled, bankAccount.RequestUnblock))
                        .ToList();
                }
                catch (Exception exception)
                {
                    logger.Error($"{exception.Message} {exception.StackTrace}");
                }
            }
           
            return bankAccountsUser;
        }
    }
}
