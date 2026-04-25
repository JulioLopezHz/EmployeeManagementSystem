using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.Data;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Services;

public class StatesService : IStatesService
{
    private readonly EmployeeManagementContext _DbContext;

    public StatesService(EmployeeManagementContext DbContext)
    {
        _DbContext = DbContext;
    }

    public async Task<List<State>?> GetStatesAsync()
    {
        try
        {
            var result = await _DbContext.States.ToListAsync();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<State?> GetStateAsync(int IdState)
    {        
        var result = await _DbContext.States.FirstOrDefaultAsync(x => x.Id == IdState);
        return result;
    }
}
