using System.Threading.Tasks;

namespace HotelBilling.Application.Interfaces
{
    public interface IChatService
    {
        Task<object> ProcessMessageAsync(string message, string userName, string role);
    }
}