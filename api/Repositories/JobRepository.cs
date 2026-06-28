using api.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories;

public class JobRepository(AppDbContext context) : GenericRepository<JobEntity>(context), IJobRepository
{
    public async Task<IEnumerable<JobEntity>> GetFilteredJobsAsync(string? projectId, string? computeType)
    {
        var query = _context.Jobs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(projectId))
        {
            query = query.Where(j => j.ProjectId == projectId);
        }

        if (!string.IsNullOrWhiteSpace(computeType))
        {
            query = query.Where(j => j.ComputeType.ToString() == computeType);
        }

        return await query.ToListAsync();
    }
}
