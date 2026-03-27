using WIRM.API.Interface;
using WIRM.API.Models.Request;

namespace WIRM.API.Services
{
    public class WeightingCalculator : IWeightingCalculator
    {
        public int Calculate(TicketForm ticketForm)
        {
            int weighting = 1;
            weighting *= ticketForm.ProductionBlock ? 10 : 1;
            weighting *= ticketForm.HasWorkAround ? 1 : 10;
            var businessDriverFactor = 1;
            switch (ticketForm.BusinessDriver)
            {
                case "Customer Retention":
                    businessDriverFactor = 5;
                    break;
                case "New Revenue":
                    businessDriverFactor = 5;
                    break;
                case "Grow Revenue":
                    businessDriverFactor = 3;
                    break;
            }
            weighting *= businessDriverFactor;
            var revenueFactor = 1;
            switch (ticketForm.Revenue)
            {
                case "Between $10K and $50K/month":
                    revenueFactor = 2;
                    break;
                case "Between $50K and $100K/month":
                    revenueFactor = 5;
                    break;
                case "Between $100K and $500K/month":
                    revenueFactor = 10;
                    break;
                case "Greater than $500K/month":
                    revenueFactor = 25;
                    break;
            }
            weighting *= revenueFactor;
            weighting *= ticketForm.HasFirmDeadline ? 5 : 1;
            return weighting;
        }
    }
}
