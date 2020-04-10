using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Infrastructure
{
    // Хранение информации об успешности операции
    public class OperationDetails
    {
        public OperationDetails(bool succedeed, string message, string property)
        {
            Succeded = succedeed;
            Message = message;
            Property = property;
        }
        public bool Succeded { get; private set; }
        public string Message { get; private set; }
        public string Property { get; private set; }
    }
}
