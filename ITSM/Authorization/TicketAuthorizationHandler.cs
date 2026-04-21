using System.Security.Claims;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Authorization;

namespace ITSM.Authorization;

public sealed class TicketAuthorizationHandler : AuthorizationHandler<TicketRequirement, Ticket>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TicketRequirement requirement,
        Ticket resource)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        // Admin can do anything with tickets.
        if (context.User.IsInRole(nameof(UserRoles.Admin)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Task.CompletedTask;

        switch (requirement.Operation)
        {
            case TicketOperations.View:
                // Coordinators can view all tickets.
                if (context.User.IsInRole(nameof(UserRoles.Coordinator)))
                    context.Succeed(requirement);
                // Technicians can view assigned tickets.
                else if (context.User.IsInRole(nameof(UserRoles.Technician)) && resource.AssignedUserId == userId)
                    context.Succeed(requirement);
                // Requesters can view their own tickets.
                else if (context.User.IsInRole(nameof(UserRoles.User)) && resource.AuthorId == userId)
                    context.Succeed(requirement);
                break;

            case TicketOperations.Reopen:
                // Requesters can reopen their own tickets; coordinators do not implicitly get this right.
                if (context.User.IsInRole(nameof(UserRoles.User)) && resource.AuthorId == userId)
                    context.Succeed(requirement);
                break;

            case TicketOperations.Unassign:
                if (context.User.IsInRole(nameof(UserRoles.Coordinator)))
                    context.Succeed(requirement);
                break;

            case TicketOperations.Assign:
                if (context.User.IsInRole(nameof(UserRoles.Coordinator)))
                    context.Succeed(requirement);
                break;

            case TicketOperations.Accept:
                // Coordinators can force-start work; technicians can accept only when unassigned or already assigned to them.
                if (context.User.IsInRole(nameof(UserRoles.Coordinator)))
                {
                    context.Succeed(requirement);
                }
                else if (context.User.IsInRole(nameof(UserRoles.Technician)) &&
                         (resource.AssignedUserId == null || resource.AssignedUserId == userId))
                {
                    context.Succeed(requirement);
                }
                break;

            case TicketOperations.Archive:
            case TicketOperations.Restore:
                // Archive/restore is a coordinator responsibility; allow assigned technicians too (for their own queue).
                if (context.User.IsInRole(nameof(UserRoles.Coordinator)))
                {
                    context.Succeed(requirement);
                }
                else if (context.User.IsInRole(nameof(UserRoles.Technician)) && resource.AssignedUserId == userId)
                {
                    context.Succeed(requirement);
                }
                break;

            case TicketOperations.Resolve:
            case TicketOperations.ChangeStatus:
            case TicketOperations.AddComment:
                // Coordinators can manage ticket lifecycle.
                if (context.User.IsInRole(nameof(UserRoles.Coordinator)))
                    context.Succeed(requirement);
                // Technicians can manage assigned tickets.
                else if (context.User.IsInRole(nameof(UserRoles.Technician)) && resource.AssignedUserId == userId)
                    context.Succeed(requirement);
                break;
        }

        return Task.CompletedTask;
    }
}
