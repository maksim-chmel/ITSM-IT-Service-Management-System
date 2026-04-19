using ITSM.Models;

namespace ITSM.Services.Automation;

public interface IAutoServiceService
{
    Task AssignTicketsByCategoryAndLoadAsync();
    Task<OperationResult> AssignTicketToAvailableUserAsync(int ticketId);
    Task ResetTicketsAsync();
}