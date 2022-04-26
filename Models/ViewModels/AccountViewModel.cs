using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace taChat.App.Models.ViewModels;

public class AccountViewModel
{
    [Required, MaxLength(30), RegularExpression(@"[a-zA-Z0-9_]+", ErrorMessage = "Should only contain alphanumeric characters and underscores")]
    public string UserName { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [HiddenInput]
    public string? ReturnUrl { get; set; }
}