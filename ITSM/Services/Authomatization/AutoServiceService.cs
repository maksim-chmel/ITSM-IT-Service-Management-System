using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.EntityFrameworkCore;
namespace ITSM.Repositories.Authomatization;
public class AutoServiceService(DBaseContext dBaseContext) : IAutoServiceService
{
    public async Task AssignTicketsByCategoryAndLoadAsync()
    {
        var ticketsToAssign = await dBaseContext.Tickets
            .Where(t => (t.Status == Status.New || t.Status == Status.Open) && t.CategoryId != null)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();

        var users = await dBaseContext.Users
            .Include(u => u.UserCategoryAssignments)
            .Include(u => u.AssignedTickets)
            .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId != null))
            .ToListAsync();

        var tasks = ticketsToAssign.Select(async ticket =>
        {
            Console.WriteLine($"Обрабатываем тикет ID: {ticket.Id}, приоритет: {ticket.Priority}, категория: {ticket.CategoryId}");

            var availableUsers = users
                .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId))
                .Select(u => new
                {
                    User = u,
                    ActiveTicketCount = u.AssignedTickets.Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled),
                    SkillLevel = (int)u.SkillLevel
                })
                .ToList();

            if (availableUsers.Any())
            {
                var selected = availableUsers
                    .OrderByDescending(u => ((int)ticket.Priority * u.SkillLevel) - u.ActiveTicketCount)
                    .First();


                await AssignTicketToUserAsync(ticket, selected.User);
            }
            else
            {
                Console.WriteLine($"Немає доступних користувачів для заявки ID: {ticket.Id}, категорія: {ticket.CategoryId}");
            }
        });

        await Task.WhenAll(tasks);
        await dBaseContext.SaveChangesAsync();
        Console.WriteLine("Всі заявки успішно призначені.");
    }

    private async Task AssignTicketToUserAsync(Models.Ticket ticket, User selectedUser)
    {
        ticket.AssignedUserId = selectedUser.Id;
        ticket.Status = Status.Progress;

        Console.WriteLine($"Заявка {ticket.Id} призначена користувачу {selectedUser.UserName}.");

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
            Console.WriteLine("Заявки успішно скинуті.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при скиданні заявок: {ex.Message}");
        }
    }
}
