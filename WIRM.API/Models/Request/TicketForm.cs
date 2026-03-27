using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Serialization;
using WIRM.API.Extensions;

namespace WIRM.API.Models.Request
{
    public class TicketForm
    {
        public string TicketType { get; set; }
        public string ChangedBy { get; set; }
        public List<string> Attachments { get; set; }
        public int TicketId { get; set; }
        public int Weighing { get; set; }
        public string? ReferenceWorkItemId { get; set; }
        public string? ReferenceWorkItemTeamProject { get; set; }
        public string? ReferenceWorkItemProductName { get; set; }
        public bool ProductionBlock { get; set; }
        public bool HasWorkAround { get; set; }
        public string BusinessDriver { get; set; }
        public bool HasFirmDeadline { get; set; }
        public DateTime? Deadline { get; set; }
        public string? DeadlineJustification { get; set; }
        public string Revenue { get; set; }
        public string CustomerName { get; set; }
        public string? OtherClientsAffected { get; set; }
        public string OriginalRequestorSide { get; set; }
        public string ReportingCustomer { get; set; }
        public string VerticalType { get; set; }
        public string? OtherVerticalType { get; set; }
        public bool IsToolRequest { get; set; }
        public string ToolOrFeatureType { get; set; }
        public string? OtherToolOrFeatureType { get; set; }
        public List<string> InputFileFormats { get; set; } = [];
        public string? OtherInputFileType { get; set; }
        public string? GeminiNumber { get; set; }
        public string? TitlePart { get; set; }
        public string TicketDescription { get; set; }
        public string AcceptanceCriteria { get; set; }
        public string BusinessCategory { get; set; }
        public string ProductType { get; set; }
        public string SubProductType { get; set; }
        public string ProductDescription { get; set; }
        public string SubProductTypeOther { get; set; }
        public string RequestEmailAddress { get; set; }
        public string ReflectedWorkItemDescription
        {
            get
            {
                var builder = new StringBuilder();
                var separator = string.Empty;
                for (int i = 0; i < 40; i++)
                    separator += "=";
                
                if (!ProductType.Equals("Lionbridge Core Technology"))
                {
                    //builder.AppendLine(separator);
                    if (InputFileFormats.Count > 0)
                        builder.AppendLine($"Input file formats: {String.Join(", ", InputFileFormats)}");

                }
                if (HasFirmDeadline)
                {
                    builder.AppendLine(separator);
                    builder.AppendLine($"Business Justification<br>{DeadlineJustification}");
                }

                builder.AppendLine(separator);
                builder.AppendLine("Description:");
                builder.AppendLine(TicketDescription.ToHtmlString());

                return builder.ToString().Replace(Environment.NewLine, "<br>");
            }
        }
        public string ReflectedWorkItemTitle
        {
            get
            {
                var dynamicTitleParts = new List<string>();
                dynamicTitleParts.Add(ReflectedProductName);
                if (!string.IsNullOrEmpty(TitlePart)) dynamicTitleParts.Add(TitlePart);
                dynamicTitleParts.Add($"{TicketType.KebabCaseToWords()} Request");
                dynamicTitleParts.Add(DateTime.Now.ToString("yyyyMMdd"));
                return $"[{CustomerName}]{String.Join('_', dynamicTitleParts)}";
            }
        }
        private string _reflectedProductName = string.Empty;
        public string ReflectedProductName
        {
            get
           {
                if (ProductType.Equals("Lionbridge Core Technology"))
                {
                    if (string.IsNullOrEmpty(SubProductType))
                    {
                        return _reflectedProductName = SubProductTypeOther;
                    }
                    _reflectedProductName = SubProductType;
                }
                else
                {
                    _reflectedProductName = $"SE - {ProductDescription}";
                }
                    return _reflectedProductName;
            }
        } 
    }
}
