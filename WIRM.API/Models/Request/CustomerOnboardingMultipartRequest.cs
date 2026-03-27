using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WIRM.API.Controllers.CustomerOnboarding;

/// <summary>
/// Bound from multipart/form-data sent by the Angular customer onboarding form.
/// Field names must match: <c>form</c> (JSON string) and optional <c>otherUsersExcel</c> (file).
/// </summary>
public class CustomerOnboardingMultipartRequest
{
    /// <summary>JSON string: full <c>getRawValue()</c> from the reactive form (camelCase).</summary>
    [FromForm(Name = "form")]
    public string Form { get; set; } = string.Empty;

    [FromForm(Name = "otherUsersExcel")]
    public IFormFile? OtherUsersExcel { get; set; }
}
