namespace ITSM;

public class UserLoad
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public int CurrentTicketWeight { get; set; }  
    public int TicketCount { get; set; } 

   
    public int Priority => CurrentTicketWeight + TicketCount * 2; 

   
    public void UpdateLoad(int ticketPriority)
    {
        CurrentTicketWeight += ticketPriority;  
        TicketCount++; 
    }

    public void DecreaseLoad(int ticketPriority)
    {
        CurrentTicketWeight -= ticketPriority;  
        TicketCount--;  
    }
}