using System.ComponentModel.DataAnnotations;

namespace Backend.Controllers;

/// <summary>HTTP request contracts. Business rules stay in FluentValidation handlers.</summary>
public sealed record RegisterRequest(
    [Required] string ControlNumber,
    [Required][MaxLength(200)] string FullName,
    [Required][EmailAddress][MaxLength(256)] string Email,
    [Required][MinLength(8)] string Password);

public sealed record LoginRequest(
    [Required] string ControlNumber,
    [Required] string Password);

public sealed record ResetPasswordRequest(
    [Required][MinLength(8)] string NewPassword);
