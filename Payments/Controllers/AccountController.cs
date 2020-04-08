using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Payments.ViewModels;
using Payments.ViewModels.Account;
using Service.Models;
using Service.Repositories;
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
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private UnitOfWork database;

        public AccountController()
        {
            database = new UnitOfWork();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                string userRole = "client";
                ApplicationRole role = await RoleManager.FindByNameAsync(userRole);
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                Microsoft.AspNet.Identity.IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, role.Name);
                    ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user,
                        DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignOut();
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
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
                ApplicationUser user = await UserManager.FindAsync(model.Email, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Неверный логин или пароль");
                }
                else if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return View("Lockout");
                }
                else
                {
                    ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user,
                        DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignOut();
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    if (String.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    return Redirect(returnUrl);
                }
            }
            ViewBag.returnUrl = returnUrl;
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
            IEnumerable<BankAccount> bankAccountsUser = null;
            IEnumerable<BankAccount> bankAccountsUserRequestUnblock = null;
            List<int> countBankAccountsUser = new List<int>();
            IEnumerable<ApplicationUser> users = UserManager.Users;
            foreach (var item in users)
            {
                bankAccountsUser = await database.BankAccounts
                .FindAllAsync(p => p.ApplicationUserId == item.Id);
                bankAccountsUserRequestUnblock = bankAccountsUser.Where(p => p.RequestUnblock == true);
                countBankAccountsUser.Add(bankAccountsUserRequestUnblock.Count());
            }
            List<DataAboutApplicationUserForAdmin> dataAboutApplicationUserForAdmins =
                new List<DataAboutApplicationUserForAdmin>();
            int i = 0;
            string adminId = User.Identity.GetUserId();
            DateTime? lockoutEndDate = null;
            foreach (var item in users)
            {
                lockoutEndDate = item.LockoutEndDateUtc;
                if(lockoutEndDate != null)
                {
                    DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                    lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
                }
                if (item.Id != adminId)
                {
                    dataAboutApplicationUserForAdmins.Add(new DataAboutApplicationUserForAdmin(
                        item.Id, item.Email, item.UserName, item.LockoutEnabled,
                        lockoutEndDate, countBankAccountsUser[i]));
                }
                i++;
            }
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
            ApplicationUser applicationUser = await UserManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            DateTime? lockoutEndDate = applicationUser.LockoutEndDateUtc;
            if (lockoutEndDate != null)
            {
                DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
            }
            UserBlockData userBlockData = new UserBlockData()
            {
                UserId = applicationUser.Id,
                Email = applicationUser.Email,
                LockoutEnabled = applicationUser.LockoutEnabled
            };
            if (applicationUser.LockoutEndDateUtc == null)
            {
                userBlockData.DateTimeBlock = DateTime.Now;
            }
            else
            {
                userBlockData.DateTimeBlock = (DateTime)lockoutEndDate;
            }
            return View(userBlockData);
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> BlockUserAccount(UserBlockData userBlockData)
        {
            if(userBlockData.DateTimeBlock <= DateTime.Now)
            {
                ModelState.AddModelError("DateTimeBlock",
                    "Дата, до которой будет заблокирован пользователь должна быть в будущем времени");
            }
            if (ModelState.IsValid)
            {
                var result = await UserManager.SetLockoutEnabledAsync(userBlockData.UserId, true);
                if (result.Succeeded)
                {
                    result = await UserManager.SetLockoutEndDateAsync(userBlockData.UserId, (DateTimeOffset)userBlockData.DateTimeBlock);
                    ApplicationUser applicationUser = await UserManager.FindByIdAsync(userBlockData.UserId);
                }
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
            ApplicationUser applicationUser = await UserManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            DateTime? lockoutEndDate = applicationUser.LockoutEndDateUtc;
            if (lockoutEndDate != null)
            {
                DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
            }
            UserBlockData userBlockData = new UserBlockData()
            {
                UserId = applicationUser.Id,
                Email = applicationUser.Email,
                LockoutEnabled = applicationUser.LockoutEnabled,
            };
            if (applicationUser.LockoutEndDateUtc == null)
            {
                userBlockData.DateTimeBlock = DateTime.Now;
            }
            else
            {
                userBlockData.DateTimeBlock = (DateTime)lockoutEndDate;
            }
            return View(userBlockData);
        }


        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> UnBlockUserAccount(UserBlockData userBlockData)
        {
            var result = await UserManager.SetLockoutEnabledAsync(userBlockData.UserId, true);
            if (result.Succeeded)
            {
                result = await UserManager.SetLockoutEndDateAsync(userBlockData.UserId, DateTimeOffset.UtcNow);
                result = await UserManager.SetLockoutEnabledAsync(userBlockData.UserId, false);
                ApplicationUser applicationUser = UserManager.FindById(userBlockData.UserId);
            }
            return RedirectToAction("DataUsers");
        }
    }
}