using EmployeeManagementSystem.Shared.Models;

namespace EmployeeManagementSystem.Interfaces;

public interface IStatesService
{
    Task<List<State>?> GetStatesAsync();
    Task<State?> GetStateAsync(int IdState);
}
