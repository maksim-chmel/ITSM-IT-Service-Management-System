namespace ITSM.Services.Automation;

public interface  IAutoServiceService
{
    public abstract Task AssignTicketsByCategoryAndLoadAsync();
    public abstract Task ResetTicketsAsync();
}