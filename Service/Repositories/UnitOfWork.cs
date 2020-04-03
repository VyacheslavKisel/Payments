using Service.Interfaces;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class UnitOfWork : IUintOfWork
    {
        private ApplicationContext applicationContext;
        private BankAccountRepository bankAccountRepository;
        private PaymentRepository paymentRepository;
        public UnitOfWork()
        {
            applicationContext = new ApplicationContext();
        }

        public IRepository<BankAccount> BankAccounts
        {
            get
            {
                if(bankAccountRepository == null)
                {
                    bankAccountRepository = new BankAccountRepository(applicationContext);
                }
                return bankAccountRepository;
            }
        }

        public IRepository<Payment> Payments
        {
            get
            {
                if (paymentRepository == null)
                {
                    paymentRepository = new PaymentRepository(applicationContext);
                }
                return paymentRepository;
            }
        }

        public async Task SaveAsync()
        {
            await applicationContext.SaveChangesAsync();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    applicationContext.Dispose();
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
