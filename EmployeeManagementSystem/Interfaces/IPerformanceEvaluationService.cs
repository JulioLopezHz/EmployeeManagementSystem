using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;

namespace EmployeeManagementSystem.Interfaces;

public interface IPerformanceEvaluationService
{
    Task<(int count, List<string> errors)> ImportPerformanceEvaluationFromExcelAsync(Stream fileStream);
    Task<int> GetPerformanceEvaluationPaginationAsync(int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null);
    Task<List<PerformanceEvaluationDto>> GetPerformanceEvaluationsAsync(int pageNumber, int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null);
    Task<PerformanceEvaluationModalDto> GetPerformanceEvaluationAsync(int Id);
    Task UpdatePerformanceEvaluationAsync(PerformanceEvaluationModalDto updatedPerformanceEvaluation);
}
