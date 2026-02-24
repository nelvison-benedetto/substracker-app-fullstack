using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.DTOs.Auth;

public sealed class LoginRequestAuth
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    //è il modelbuilder che crea l'obj, non il controller, le proprietà vengono settate dopo la costruzione
}
