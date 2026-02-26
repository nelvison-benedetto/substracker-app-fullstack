using SubSnap.Application.UseCases.Auth.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.Ports.Auth;

public interface ILoginHandler
{
    Task<LoginResult> Handle(LoginCommand cmd, CancellationToken ct);
}
