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
    public class PaymentRepository : IRepository<Payment>
    {
        private ApplicationContext applicationContext;

        public PaymentRepository(ApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        public async Task<Payment> GetAsync(int id)
        {
            return await applicationContext.Payments.FindAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await applicationContext.Payments.ToListAsync();
        }

        public void Create(Payment payment)
        {
            applicationContext.Payments.Add(payment);
        }

        public void Update(Payment payment)
        {
            applicationContext.Entry(payment).State = System.Data.Entity.EntityState.Modified;
        }

        public async Task<Payment> FindAsync(Func<Payment, bool> predicate)
        {
            IEnumerable<Payment> payments = await GetAllAsync();
            Payment payment = payments.FirstOrDefault(predicate);
            return payment;
        }

        public async Task<IEnumerable<Payment>> FindAllAsync(Func<Payment, bool> predicate)
        {
            IEnumerable<Payment> result = await applicationContext.Payments.ToListAsync();
            return result.Where(predicate);
        }
    }
}
