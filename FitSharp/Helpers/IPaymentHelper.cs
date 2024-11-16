using Braintree;

namespace FitSharp.Helpers
{
    public interface IPaymentHelper
    {
        IBraintreeGateway CreateGateway();

        IBraintreeGateway GetGateway();
    }
}
