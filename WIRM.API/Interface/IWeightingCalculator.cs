using WIRM.API.Models.Request;

namespace WIRM.API.Interface
{
    public interface IWeightingCalculator 
    {
        int Calculate(TicketForm ticketForm);
    }
}
