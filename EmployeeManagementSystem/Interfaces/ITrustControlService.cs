using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;

namespace EmployeeManagementSystem.Interfaces;

public interface ITrustControlService
{
    Task<(int count, List<string> errors)> ImportTrustControlsFromExcelAsync(Stream fileStream);
    Task<int> GetTrustControlsPaginationAsync(int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null);
    Task<List<TrustControlDto>> GetTrustControlsAsync(int pageNumber, int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null);
    Task<Employee> GetTrustControlAsync(int Id);
}
