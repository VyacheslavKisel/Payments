using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Payments.ViewModels;
using Payments.ViewModels.Account;
using Service.Models;
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

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // исправить на асинхронный метод
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                string userRole = "client";
                ApplicationRole role = RoleManager.FindByName(userRole);
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                Microsoft.AspNet.Identity.IdentityResult result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, role.Name);
                    ClaimsIdentity claim = UserManager.CreateIdentity(user,
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

        //Не передаем ViewModel
        [Authorize(Roles = "administrator")]
        public ActionResult DataUsers()
        {
            return View(UserManager.Users);
        }

        //Блокировка пользователя
        [Authorize(Roles = "administrator")]
        public ActionResult BlockUserAccount(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            UserBlockData userBlockData = new UserBlockData()
            {
                UserId = applicationUser.Id,
                Email = applicationUser.Email,
                LockoutEnabled = applicationUser.LockoutEnabled,
            };
            if (applicationUser.LockoutEndDateUtc == null)
            {
                userBlockData.DateTimeBlock = DateTime.UtcNow;
            }
            else
            {
                userBlockData.DateTimeBlock = (DateTime)applicationUser.LockoutEndDateUtc;
            }
            return View(userBlockData);
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public ActionResult BlockUserAccount(UserBlockData userBlockData)
        {
            if (ModelState.IsValid)
            {
                var result = UserManager.SetLockoutEnabled(userBlockData.UserId, true);
                if (result.Succeeded)
                {
                    result = UserManager.SetLockoutEndDate(userBlockData.UserId, (DateTimeOffset)userBlockData.DateTimeBlock);
                    ApplicationUser applicationUser = UserManager.FindById(userBlockData.UserId);
                    //logger.Info($"Был заблокирован пользователь, у которого логин {applicationUser.UserName} и почта {applicationUser.Email}");
                }
                return RedirectToAction("DataUsers");
            }
            return View();
        }

        // Разблокировка пользователей
        [Authorize(Roles = "administrator")]
        public ActionResult UnBlockUserAccount(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            UserBlockData userBlockData = new UserBlockData()
            {
                UserId = applicationUser.Id,
                Email = applicationUser.Email,
                LockoutEnabled = applicationUser.LockoutEnabled,
            };
            if (applicationUser.LockoutEndDateUtc == null)
            {
                userBlockData.DateTimeBlock = DateTime.UtcNow;
            }
            else
            {
                userBlockData.DateTimeBlock = (DateTime)applicationUser.LockoutEndDateUtc;
            }
            return View(userBlockData);
        }


        [HttpPost]
        [Authorize(Roles = "administrator")]
        public ActionResult UnBlockUserAccount(UserBlockData userBlockData)
        {
            var result = UserManager.SetLockoutEnabled(userBlockData.UserId, true);
            if (result.Succeeded)
            {
                result = UserManager.SetLockoutEndDate(userBlockData.UserId, DateTimeOffset.UtcNow);
                result = UserManager.SetLockoutEnabled(userBlockData.UserId, false);
                ApplicationUser applicationUser = UserManager.FindById(userBlockData.UserId);
                //logger.Info($"Был разблокирован пользователь, у которого логин {applicationUser.UserName} и почта {applicationUser.Email}");
            }
            return RedirectToAction("DataUsers");
        }
    }
}