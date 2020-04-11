using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Payments.BLL.DTO.Account;
using Payments.BLL.Infrastructure;
using Payments.BLL.Interfaces;
using Payments.BLL.Services;
using Payments.ViewModels;
using Payments.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Payments.Controllers
{
    public class AccountController : Controller
    {
        private IBankAccountService bankAccountService;

        public AccountController(IBankAccountService bankAccountService)
        {
            this.bankAccountService = bankAccountService;
        }

        private IUserService UserService
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<IUserService>();
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                UserDTO userDTO = new UserDTO
                {
                    Email = model.Email,
                    UserName = model.Email,
                    Password = model.Password,
                    Role = "client"
                };
                OperationDetails operationDetails = await UserService.Create(userDTO);
                if (operationDetails.Succeded)
                {
                    ClaimsIdentity claim = await UserService.Authenticate(userDTO);
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(operationDetails.Property, operationDetails.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                UserDTO userDTO = new UserDTO
                {
                    Email = model.Email,
                    Password = model.Password
                };
                ClaimsIdentity claim = await UserService.Authenticate(userDTO);
                if (claim == null)
                {
                    ModelState.AddModelError("", "Неверный логин или пароль");
                }
                else if (await UserService.IsLockedOutAsync(
                    await UserService.FindUserIdAsync(model.Email)))
                {
                    return View("Lockout");
                }
                else
                {
                    AuthenticationManager.SignOut();
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> DataUsers()
        {
            var users = UserService.GetUsers();
            string adminId = User.Identity.GetUserId();
            var dataAboutApplicationUserForAdminsDTO = await UserService.GetDataAboutUsersAsync(users, adminId);
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<FullDataUserForAdminDTO,
              DataAboutApplicationUserForAdmin>()).CreateMapper();
            var dataAboutApplicationUserForAdmins = mapper.Map<IEnumerable<FullDataUserForAdminDTO>,
                 IEnumerable<DataAboutApplicationUserForAdmin>>(dataAboutApplicationUserForAdminsDTO);
            return View(dataAboutApplicationUserForAdmins);
        }

        // Блокировка пользователя
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BlockUserAccount(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            UserBlockDataDTO userBlockDataDTO = await UserService.FindUserForBlockAsync(id);
            if (userBlockDataDTO == null)
            {
                return HttpNotFound();
            }
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<UserBlockDataDTO,
              UserBlockData>()).CreateMapper();
            var userBlockData = mapper.Map<UserBlockDataDTO,
                 UserBlockData>(userBlockDataDTO);
            return View(userBlockData);
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BlockUserAccount(UserBlockData userBlockData)
        {
            try
            {
                UserService.CheckDateBlock(userBlockData.DateTimeBlock);
            }
            catch (ValidationBusinessLogicException exception)
            {
                ModelState.AddModelError(exception.Property, exception.Message);
            }
            if (ModelState.IsValid)
            {
                var mapper = new MapperConfiguration(cfg => cfg.CreateMap<UserBlockData,
              UserBlockDataDTO>()).CreateMapper();
                var userBlockDataDTO = mapper.Map<UserBlockData,
                     UserBlockDataDTO>(userBlockData);
                await UserService.BlockUserAccount(userBlockDataDTO);
                return RedirectToAction("DataUsers");
            }
            return View(userBlockData);
        }

        // Разблокировка пользователей
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> UnBlockUserAccount(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            UserBlockDataDTO userBlockDataDTO = await UserService.FindUserForUnBlockAsync(id);
            if (userBlockDataDTO == null)
            {
                return HttpNotFound();
            }
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<UserBlockDataDTO,
              UserBlockData>()).CreateMapper();
            var userBlockData = mapper.Map<UserBlockDataDTO,
                 UserBlockData>(userBlockDataDTO);
            return View(userBlockData);
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> UnBlockUserAccount(UserBlockData userBlockData)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<UserBlockData,
              UserBlockDataDTO>()).CreateMapper();
            var userBlockDataDTO = mapper.Map<UserBlockData,
                 UserBlockDataDTO>(userBlockData);
            await UserService.UnblockUserAccount(userBlockDataDTO);
            return RedirectToAction("DataUsers");
        }
    }
}