using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IUintOfWork : IDisposable
    {
        IRepository<BankAccount> BankAccounts { get; }
        IRepository<Payment> Payments { get; }
        Task SaveAsync();
    }
}
