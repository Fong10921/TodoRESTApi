using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace TodoRESTApi.Controllers.V2.RESTApi;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("2.0")]
public class Todo : ControllerBase
{
    [HttpGet("Todo")]
    public IActionResult Get()
    {
        return Ok(new { message = "This is version 2.0 of the Document API" });
    }
}