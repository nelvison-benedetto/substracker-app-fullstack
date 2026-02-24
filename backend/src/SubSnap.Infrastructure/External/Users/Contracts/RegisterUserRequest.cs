using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.DTOs.External.Requests.Users;

//validati con FluentValidation, non consoscono Domain
public sealed class RegisterUserRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}
