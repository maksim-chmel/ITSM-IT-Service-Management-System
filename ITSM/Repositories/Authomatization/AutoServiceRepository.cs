using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSM;
public class AutoServiceRepository(DBaseContext dBaseContext) : IAutoServiceRepository
{
    public async Task AssignTicketsByCategoryAndLoadAsync()
{
   
    var ticketsToAssign = await dBaseContext.Tickets
        .Where(t => (t.Status == Status.New || t.Status == Status.Open) && t.CategoryId != null)
        .OrderByDescending(t => t.Priority)  
        .ToListAsync();

  
    var users = await dBaseContext.Users
        .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId != null))
        .Include(u => u.UserCategoryAssignments)
        .ToListAsync();

   
    var tasks = ticketsToAssign.Select(async ticket =>
    {
        Console.WriteLine($"Обрабатываем тикет с ID: {ticket.Id}, категория: {ticket.CategoryId}, приоритет: {ticket.Priority}");

        
        var availableUsers = users.Where(u =>
            u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId)).ToList();

        
        if (availableUsers.Any())
        {
           
            var selectedUser = availableUsers
                .OrderBy(u => u.AssignedTickets.Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled))  // Сортируем по количеству назначенных тикетов
                .FirstOrDefault();

            if (selectedUser != null)
            {
                
                await AssignTicketToUserAsync(ticket, selectedUser);
            }
        }
        else
        {
            Console.WriteLine($"Не найден пользователь для тикета {ticket.Id}, категория: {ticket.CategoryId}.");
        }
    });

   
    await Task.WhenAll(tasks);

   
    await dBaseContext.SaveChangesAsync();

    Console.WriteLine("Все тикеты обработаны.");
}

private async Task AssignTicketToUserAsync(Ticket ticket, User selectedUser)
{
    
    ticket.AssignedUserId = selectedUser.Id;
    ticket.Status = Status.Progress;  

    
    Console.WriteLine($"Тикет {ticket.Id} назначен пользователю {selectedUser.UserName}.");

  
    selectedUser.AssignedTickets.Add(ticket);
}

public async Task ResetTicketsAsync()
{
  
    var ticketsToReset = await dBaseContext.Tickets
        .Where(t => t.Status != Status.Resolved && t.Status != Status.Canceled)
        .ToListAsync();

    
    foreach (var ticket in ticketsToReset)
    {
        ticket.AssignedUserId = null;  
        ticket.Status = Status.New;    
    }

   
    try
    {
        await dBaseContext.SaveChangesAsync();
        Console.WriteLine("Тикеты сброшены.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при сбросе тикетов: {ex.Message}");
    }
}

}
