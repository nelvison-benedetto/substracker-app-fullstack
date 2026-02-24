using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.DTOs.Auth;

public sealed class LogoutRequestAuth
{
    public required string RefreshToken { get; init; }
}
