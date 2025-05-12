namespace ITSM;

public interface  IAutoServiceService
{
    public abstract Task AssignTicketsByCategoryAndLoadAsync();
    public abstract Task ResetTicketsAsync();
}