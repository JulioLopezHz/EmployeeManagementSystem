namespace EmployeeManagementSystem.Interfaces;

public interface IPayrollRecordService
{
    Task<(int count, List<string> errors)> ImportPayrollRecordsFromExcelAsync(Stream fileStream);
}
