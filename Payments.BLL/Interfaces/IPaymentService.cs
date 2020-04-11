using Payments.BLL.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentBankAccountDTO>> PaymentData(int id);
        Task PreparePayment(PreparationPaymentDTO preparationPaymentDTO);
        Task<IEnumerable<PreparedPaymentDTO>> FindPreparedPayments(IEnumerable<int> bankAccountsIds);
        Task ReplenishBankAccount(ReplenishBankAccountDTO replenishBankAccountDTO);
        Task Pay(IEnumerable<PreparedPaymentDTO> preparedPaymentDTO);
        Task RejectPayment(int? id);

    }
}
