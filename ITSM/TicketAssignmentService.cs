using ITSM.DB;
using ITSM.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITSM;

public class TicketAssignmentService
{
    private readonly DBaseContext _dBaseContext;

    public TicketAssignmentService(DBaseContext dBaseContext)
    {
        _dBaseContext = dBaseContext;
    }

    public async Task MassAssignTicketsAsync(TicketPriority? priority = null)
    {
        // Получаем все тикеты в статусах "New" или "Open"
        var ticketsToAssign = await _dBaseContext.Tickets
            .Where(t => (t.Status == Status.New || t.Status == Status.Open) && t.CategoryId != null)
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .AsNoTracking()
            .ToListAsync();

        // Получаем всех пользователей с их категориями и назначенными тикетами
        var candidates = await _dBaseContext.Users
            .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId != null))
            .Include(u => u.AssignedTickets)
            .AsNoTracking()
            .ToListAsync();

        // Создаем очередь с приоритетами
        var userQueue = new PriorityQueue<UserLoad, int>();

        // Инициализируем очередь с пользователями
        foreach (var user in candidates)
        {
            var userLoad = new UserLoad
            {
                UserId = user.Id,
                UserName = user.UserName,
                TicketCount = user.AssignedTickets.Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled),
            };

            // Считаем вес тикетов, назначенных пользователю
            userLoad.CurrentTicketWeight = user.AssignedTickets
                .Where(t => t.Status != Status.Resolved && t.Status != Status.Canceled)
                .Sum(t => (int)t.Priority);

            userQueue.Enqueue(userLoad, userLoad.Priority);
        }

        // Фильтруем тикеты по приоритету, если оно задано
        if (priority.HasValue)
        {
            ticketsToAssign = ticketsToAssign.Where(t => t.Priority == priority).ToList();
        }

        // Назначаем тикеты
        foreach (var ticket in ticketsToAssign)
        {
            // Получаем пользователя с минимальной нагрузкой
            if (userQueue.Count > 0)
            {
                var selectedUser = userQueue.Dequeue();  // Извлекаем пользователя с минимальной нагрузкой

                // Назначаем тикет пользователю
                ticket.AssignedUserId = selectedUser.UserId;
                ticket.Status = Status.Progress;  // Изменяем статус тикета на "В процессе"

                // Обновляем нагрузку пользователя после назначения тикета
                selectedUser.UpdateLoad((int)ticket.Priority);

                // Пересчитываем приоритет пользователя и возвращаем его обратно в очередь
                userQueue.Enqueue(selectedUser, selectedUser.Priority);
            }
        }

        // Сохраняем изменения
        await _dBaseContext.SaveChangesAsync();
    }
}