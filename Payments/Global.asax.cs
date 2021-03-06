﻿using Ninject;
using Ninject.Modules;
using Ninject.Web.Mvc;
using Payments.BLL.Infrastructure;
using Payments.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Payments
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            NinjectModule bankAccountModule = new BankAccountModule();
            NinjectModule paymentModule = new PaymentModule();
            NinjectModule userModule = new UserModule();
            NinjectModule serviceModule = new ServiceModule();
            var kernel = new StandardKernel(bankAccountModule, paymentModule, userModule, serviceModule);
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
        }
    }
}
