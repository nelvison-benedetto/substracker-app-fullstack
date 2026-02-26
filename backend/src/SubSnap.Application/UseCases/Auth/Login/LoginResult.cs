using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.UseCases.Auth.Login;

public sealed record LoginResult( string AccessToken, string RefreshToken );
