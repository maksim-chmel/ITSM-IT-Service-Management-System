namespace ITSM.Services.Authomatization;

public interface  IAutoServiceService
{
    public abstract Task AssignTicketsByCategoryAndLoadAsync();
    public abstract Task ResetTicketsAsync();
}