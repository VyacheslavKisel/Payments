using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Infrastructure
{
    public class ValidationBusinessLogicException : Exception
    {
        public string Property { get; protected set; }
        public ValidationBusinessLogicException(string message, string property) : base(message)
        {
            Property = property;
        }
    }
}
