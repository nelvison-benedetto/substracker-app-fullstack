using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.DTOs.Application.Results.Users;

//questi sono usati dai Services, poi mappati in Responses
public sealed record UserResult(
    Guid Id,
    string Email
);


