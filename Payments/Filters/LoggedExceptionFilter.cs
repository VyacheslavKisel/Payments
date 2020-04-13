using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Payments.Filters
{
    public class LoggedExceptionFilter : FilterAttribute, IExceptionFilter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext exceptionContext)
        {
            if(!exceptionContext.ExceptionHandled && exceptionContext.Exception != null)
            {
                logger.Error($"{exceptionContext.Exception.Message} {exceptionContext.Exception.StackTrace}");

                exceptionContext.ExceptionHandled = true;
                exceptionContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary(exceptionContext.Controller.ViewData)
                    {
                        Model = exceptionContext.Exception.Message
                    }
                };
            }
        }
    }
}