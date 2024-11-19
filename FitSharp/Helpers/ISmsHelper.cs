namespace FitSharp.Helpers
{
    public interface ISmsHelper
    {
        void SendSms(string toPhoneNumber, string message);
    }
}
