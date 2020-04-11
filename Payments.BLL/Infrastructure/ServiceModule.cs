using Ninject.Modules;
using Service.Interfaces;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Infrastructure
{
    public class ServiceModule : NinjectModule
    {
        public ServiceModule() 
        {
        }

        public override void Load()
        {
            Bind<IUnitOfWork>().To<UnitOfWork>();
        }
    }
}
