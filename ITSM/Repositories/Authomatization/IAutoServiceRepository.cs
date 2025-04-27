namespace ITSM;

public interface  IAutoServiceRepository
{
    public abstract Task AssignTicketsByCategoryAndLoadAsync();
    public abstract Task ResetTicketsAsync();
}