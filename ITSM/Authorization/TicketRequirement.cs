using Microsoft.AspNetCore.Authorization;

namespace ITSM.Authorization;

public sealed record TicketRequirement(string Operation) : IAuthorizationRequirement;

