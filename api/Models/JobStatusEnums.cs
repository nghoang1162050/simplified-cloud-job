namespace api.Models;

public enum JobStatusEnums
{
    /// <summary>
    /// Job is initialized and saved to the database, waiting to be queued.
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Job is placed in the processing queue, waiting for compute resources.
    /// </summary>
    Queued = 2,

    /// <summary>
    /// Job is actively executing on the compute node.
    /// </summary>
    Running = 3,

    /// <summary>
    /// Job executed successfully, and output files have been generated.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Job encountered an error during runtime (e.g., code crash, OOM, timeout).
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Job was manually cancelled by the user or terminated by the system.
    /// </summary>
    Cancelled = 6
}
