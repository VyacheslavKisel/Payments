using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    // Интерфейс репозитория
    public interface IRepository<T> where T : class
    {
        Task<T> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllIncludeLinkedDataAsync();
        Task<T> FindAsync(Func<T, Boolean> predicate);
        Task<IEnumerable<T>> FindAllAsync(Func<T, Boolean> predicate);
        void Create(T item);
        void Update(T item);
    }
}
