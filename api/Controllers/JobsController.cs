using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController(IJobServices jobServices) : ControllerBase
{
    private readonly IJobServices _jobServices = jobServices;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Submit and trigger a new cloud job")]
    public async Task<IActionResult> Post([FromForm] CreateJobRequest request)
    {
        var response = await _jobServices.SubmitJobAsync(request);
        return Created($"api/jobs/{response.Data?.JobId}", response);
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> Get([FromRoute] string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Job ID is required in the path parameter."));
        }

        var response = await _jobServices.GetJobStatusAsync(jobId);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost("{id}/complete")]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Webhook called by AWS EC2 to finalize job and process billing")]
    public async Task<IActionResult> CompleteJob([FromRoute] string id, [FromForm] CompleteJobRequest request)
    {
        var response = await _jobServices.CompleteJobAsync(id, request);
        return Ok(response);
    }

    [HttpGet("billing-summary")]
    [EndpointSummary("Get billing summary")]
    public async Task<IActionResult> GetBillingSummary([FromQuery] BillingSummaryFilterRequest request)
    {
        var response = await _jobServices.GetBillingSummaryAsync(request);
        return Ok(response);
    }
}
