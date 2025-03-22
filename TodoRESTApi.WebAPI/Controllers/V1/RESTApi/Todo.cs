using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace TodoRESTApi.WebAPI.Controllers.V1.RESTApi;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
public class Todo : ControllerBase
{
    [HttpGet("Todo")]
    public IActionResult Get()
    {
        return Ok(new { message = "This is version 1.0 of the Document API" });
    }
}