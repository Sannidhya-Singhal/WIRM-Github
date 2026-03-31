using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text.Json.Serialization;

namespace WIRM.API.Controllers.CustomerOnboarding;

/// <summary>
/// Shape of the JSON inside multipart field <c>form</c> (Angular <c>getRawValue()</c>).
/// </summary>
public class CustomerOnboardingFormDto
{
    //Step 1 - Account Details
    public string CustomerGroupName { get; set; } = string.Empty;
    public string CustomerAccountName { get; set; } = string.Empty;
    public string CodaCode { get; set; } = string.Empty;
    public bool IsExistingCustomer { get; set; } = false;
    public bool IsFederatedAccessSSO { get; set; } = false;


    // Step 2 - Optional Module Settings
    public bool Validate { get; set; } = false;
    public bool Engage { get; set; } = false;
    public bool Insights { get; set; } = false;
    public bool Apps { get; set; } = false;
    public bool Msp { get; set; } = false;

    //Step 3 - User and Roles
    public string PrimaryUserFirstName { get; set; } = string.Empty;
    public string PrimaryUserLastName { get; set; } = string.Empty;
    public string PrimaryUserEmailAddress { get; set; } = string.Empty;
    public string PrimaryUserPreferredLanguage { get; set; } = string.Empty;
    public List<OtherUserRowDto> OtherUsers { get; set; } = new();

    //Step 4 - Process Specifics
    public List<CustomizedTemplateRowDto> CustomizedTemplates { get; set; } = new();

    //Step 5- Modifiers
    public List<ModifierRowDto> BusinessModifiers { get; set; } = new();
    public List<ModifierRowDto> FinanceModifiers { get; set; } = new();
    public List<ModifierRowDto> ProcessModifiers { get; set; } = new();

    //Step 6 - Considerations
    public string ConsiderAnythingElse { get; set; } = string.Empty;
    /// <summary>Date from <c>type="date"</c> input (<c>yyyy-MM-dd</c>).</summary>
    //public string ConsiderUrgentDeployment { get; set; } = string.Empty;
    public string ConsiderOperationalOwnerAccount { get; set; } = string.Empty;

    /// <summary>Angular control name typo preserved in JSON: <c>considerAccountManager</c>.</summary>
    [JsonPropertyName("considerAccountManager")]
    public string ConsiderAccountManager { get; set; } = string.Empty;
    public string ConsiderMigrationPoc { get; set; } = string.Empty;

    [JsonIgnore]
    public string TicketTitle
    {
        get
        {
            return string.Join(" - ", new[] { "ONBOARD REQUEST", $"{CustomerGroupName} / {CustomerAccountName}" });
        }
    }
    [JsonIgnore]
    public string LoggedInUserEmail { get; set; } = string.Empty;
}

public class OtherUserRowDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PrefferedLanguage { get; set; } = string.Empty;
}

public class CustomizedTemplateRowDto
{
    public string DesiredName { get; set; } = string.Empty;
    public string ExistingAuroraProcess { get; set; } = string.Empty;
    public List<string> AvailableAddOns { get; set; } = new();
    public List<string> WorkType { get; set; } = new();
    public List<string> Service { get; set; } = new();
}

public class ModifierRowDto
{
    public string Name { get; set; } = string.Empty;
    public string Values { get; set; } = string.Empty;
    public string DetailsPurpose { get; set; } = string.Empty;
    public string ExpectedBehaviorWhenSelected { get; set; } = string.Empty;
}
