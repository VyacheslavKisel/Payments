using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Payments.Filters
{
    public class LoggedExceptionFilter : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext exceptionContext)
        {
            if(!exceptionContext.ExceptionHandled && exceptionContext.Exception != null)
            {
                exceptionContext.ExceptionHandled = true;
                exceptionContext.Result = new HttpNotFoundResult();
            }
        }
    }
}