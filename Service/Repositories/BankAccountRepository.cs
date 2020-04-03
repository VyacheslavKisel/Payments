using Service.Interfaces;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class BankAccountRepository : IRepository<BankAccount>
    {
        private ApplicationContext applicationContext;

        public BankAccountRepository(ApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        public async Task<BankAccount> GetAsync(int id)
        {
            return await applicationContext.BankAccounts.FindAsync(id);
        }

        public async Task<IEnumerable<BankAccount>> GetAllAsync()
        {
            return await applicationContext.BankAccounts.ToListAsync();
        }

        public void Create(BankAccount bankAccount)
        {
            applicationContext.BankAccounts.Add(bankAccount);
        }

        public void Update(BankAccount bankAccount)
        {
            applicationContext.Entry(bankAccount).State = System.Data.Entity.EntityState.Modified;
        }

        public async Task<BankAccount> FindAsync(Func<BankAccount, Boolean> predicate)
        {
            IEnumerable<BankAccount> bankAccounts = await GetAllAsync();
            BankAccount bankAccount = bankAccounts.FirstOrDefault(predicate);
            return bankAccount;
        }

        public async Task<IEnumerable<BankAccount>> FindAllAsync(Func<BankAccount, Boolean> predicate)
        {
            IEnumerable<BankAccount> result = await applicationContext.BankAccounts.ToListAsync();
            return result.Where(predicate);
        }
    }
}
