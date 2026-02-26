using SubSnap.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.UseCases.Auth.Logout;

public sealed record LogoutCommand( UserId UserId, string RefreshToken );
