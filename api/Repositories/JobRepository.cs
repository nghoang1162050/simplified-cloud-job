using api.Entities;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories;

public class JobRepository(AppDbContext context) : GenericRepository<JobEntity>(context), IJobRepository
{
    public async Task<PagedResult<JobEntity>> GetFilteredJobsAsync(
      string? projectId,
      string? computeType,
      int page,
      int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        IQueryable<JobEntity> query = _context.Jobs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(projectId))
        {
            query = query.Where(j => j.ProjectId == projectId);
        }

        if (!string.IsNullOrWhiteSpace(computeType) &&
            Enum.TryParse<ComputeTypeEnums>(computeType, true, out var parsedComputeType))
        {
            query = query.Where(j => j.ComputeType == parsedComputeType);
        }
        else if (!string.IsNullOrWhiteSpace(computeType))
        {
            return new PagedResult<JobEntity>(Enumerable.Empty<JobEntity>(), page, pageSize, 0);
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<JobEntity>(items, page, pageSize, totalCount);
    }

    public async Task<JobEntity?> GetByHashAsync(string requestHash)
    {
        return await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.RequestHash == requestHash);
    }
}
