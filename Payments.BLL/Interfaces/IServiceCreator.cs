using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Interfaces
{
    public interface IServiceCreator
    {
        IUserService CreateUserService();
    }
}
