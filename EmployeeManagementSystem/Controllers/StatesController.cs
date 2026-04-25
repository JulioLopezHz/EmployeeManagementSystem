using EmployeeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers;

[Route("[controller]")]
[ApiController]
public class StatesController : ControllerBase
{
    private readonly IStatesService _StatesService;

    public StatesController(IStatesService StatesService)
    {
        _StatesService = StatesService;
    }

    [HttpGet("GetAllStates")]
    public async Task<ActionResult> GetAllStates()
    {
        var result = await _StatesService.GetStatesAsync();
        return result != null ? Ok(result) : StatusCode(500);
    }

    [HttpGet("GetState")]
    public async Task<ActionResult> GetState(int IdState)
    {
        var result = await _StatesService.GetStateAsync(IdState);
        return result != null ? Ok(result) : StatusCode(500);
    }
}
