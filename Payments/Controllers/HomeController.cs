using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Payments.BLL.DTO.BankAccount;
using Payments.BLL.Services;
using Payments.ViewModels.BankAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Payments.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}