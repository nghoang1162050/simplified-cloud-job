using api.Entities;

namespace api.Repositories;

public interface IJobRepository : IGenericRepository<JobEntity>
{
    Task<IEnumerable<JobEntity>> GetFilteredJobsAsync(string? projectId, string? computeType);
}
