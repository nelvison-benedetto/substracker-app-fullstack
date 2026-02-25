using SubSnap.Core.DTOs.Application.Commands.Users;
using SubSnap.Core.DTOs.Application.Results.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.Contracts.Services;

public interface IUserService
{
    Task<UserResult> RegisterAsync(RegisterUserCommand command);
}
