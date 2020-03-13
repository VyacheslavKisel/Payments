using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models
{
    // Модель роли пользователя
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole()
        {
        }
    }
}
