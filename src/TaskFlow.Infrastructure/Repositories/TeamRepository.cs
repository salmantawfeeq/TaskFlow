using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Team?> GetWithMembersAsync(int id) =>
        await DbSet
            .Include(t => t.Department)
            .Include(t => t.TeamLead)
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.Projects)
            .FirstOrDefaultAsync(t => t.Id == id);
}
