using api.Entities;
using api.Models;

namespace api.Repositories;

public interface IJobRepository : IGenericRepository<JobEntity>
{
    public Task<PagedResult<JobEntity>> GetFilteredJobsAsync(
      string? projectId,
      string? computeType,
      int page,
      int pageSize);
}
