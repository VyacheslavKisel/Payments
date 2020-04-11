using Payments.BLL.Interfaces;
using Service.Interfaces;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Services
{
    // Фабрика, которая создает UserService
    public class ServiceCreator : IServiceCreator
    {
        public IUserService CreateUserService()
        {
            return new UserService(new UnitOfWork());
        }
    }
}
