using EmployeeManagementSystem.Shared.DTOs;

namespace EmployeeManagementSystem.Interfaces;

public interface IEmployeesService
{
    Task<(int count, List<string> errors)> ImportUsersFromExcelAsync(Stream fileStream);
    Task<int> GetEmployeesPaginationAsync(int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null);
    Task<List<EmployeeDto>> GetEmployeesAsync(int pageNumber, int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null);
}
