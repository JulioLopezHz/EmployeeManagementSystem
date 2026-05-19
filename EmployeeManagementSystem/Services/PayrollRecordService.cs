using ClosedXML.Excel;
using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.Data;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EmployeeManagementSystem.Services;

public class PayrollRecordService : IPayrollRecordService
{
    private readonly EmployeeManagementContext _DbContext;
    private CultureInfo _cultureInfo = new CultureInfo("es-MX");
    private const int PageSize = 10;

    public PayrollRecordService(EmployeeManagementContext DbContext)
    {
        _DbContext = DbContext;
    }

    /// <summary>
    /// Procesa la información de un archivo Excel para crear nuevos empleados en la base de datos. El archivo debe tener una estructura específica con los campos en el orden correcto, de lo contrario se generarán errores. Si un empleado con el mismo Id ya existe en la base de datos, no se creará nuevamente y se agregará un mensaje de error indicando que el elemento ya existe.
    /// </summary>
    /// <param name="fileStream">Stream del archivo Excel</param>
    /// <returns>Una tupla que contiene el número de empleados importados y una lista de errores encontrados durante la importación</returns>
    public async Task<(int count, List<string> errors)> ImportPayrollRecordsFromExcelAsync(Stream fileStream)
    {
        var payrollRecords = new List<PayrollRecord>();
        var errors = new List<string>();

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1); //Obtiene la primera Hoja 1
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Ignora el encabezado (Fila 1)
            var errorIds = new List<int>();

            foreach (var row in rows)
            {
                try
                {
                    var employeeNumber = row.Cell(1).GetValue<int>();
                    var payrollPositionId = !row.Cell(15).IsEmpty() ? row.Cell(15).GetValue<int>() : 0;

                    if (employeeNumber == 0)
                    {
                        errors.Add($"Fila {row.RowNumber()}: El número de empleado es requerido.");
                        continue;
                    }
                    // Valida que la FK exista en la Base de Datos, si no existe, se agrega un mensaje de error
                    var employeeExists = await _DbContext.Employees.AnyAsync(e => e.Id == employeeNumber);
                    if (!employeeExists)
                    {
                        errors.Add($"Fila {row.RowNumber()}: El empleado con número '{employeeNumber}' no existe.");
                        continue;
                    }

                    var payrollRecord = new PayrollRecord
                    {
                        EmployeeNumber = employeeNumber,
                        StartDate = !row.Cell(2).IsEmpty() ? getDate(row.Cell(2).GetValue<string>()) : null,
                        IsDirectDesignation = !row.Cell(3).IsEmpty() ? getBool(row.Cell(3).GetValue<string>()) : null,
                        ProfessionalCareerServiceId = !row.Cell(5).IsEmpty() ? row.Cell(5).GetValue<int>() : null,
                        ProfessionalCareerServiceYear = !row.Cell(6).IsEmpty() ? row.Cell(6).GetValue<int>() : null,
                        AssignmentArea = !row.Cell(7).IsEmpty() ? row.Cell(7).GetValue<string>() : null,
                        PayrollPeriod = !row.Cell(8).IsEmpty() ? getDate(row.Cell(8).GetValue<string>()) : null,
                        IsEnabled = !row.Cell(9).IsEmpty() ? getBool(row.Cell(9).GetValue<string>()) ?? false : false,
                        PayrollBranchId = !row.Cell(11).IsEmpty() ? row.Cell(11).GetValue<int>() : null,
                        PaymentAreaId = !row.Cell(13).IsEmpty() ? row.Cell(13).GetValue<int>() : null,
                        PayrollPositionRecords = payrollPositionId >  0 ? new List<PayrollPositionRecord>{
                            new PayrollPositionRecord
                            {
                                EmployeeNumber = employeeNumber,
                                PayrollPositionId = payrollPositionId,
                            }
                        } : new List<PayrollPositionRecord>()
                    };

                    payrollRecords.Add(payrollRecord);
                }
                catch (Exception ex)
                {
                    // Si ocurre un error al convertir los datos, se agrega un mensaje de error específico para esa fila
                    errors.Add($"Fila {row.RowNumber()}: Error al convertir los datos. {ex.Message}");
                }
            }

            if (payrollRecords.Any())
            {
                _DbContext.PayrollRecords.AddRange(payrollRecords);
                await _DbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            errors.Add($"ERROR al querer guardar la información en la base de datos: {ex.Message}");
        }
        return (payrollRecords.Count, errors);
    }

    private DateOnly? getDate(string fechaStr)
    {
        DateOnly? fecha = null;

        if (DateTime.TryParse(fechaStr, _cultureInfo, DateTimeStyles.None, out DateTime resultado))
        {
            fecha = DateOnly.FromDateTime(resultado);
        }
        return fecha;
    }

    private bool? getBool(string boolStr)
    {
        bool? response = null;
        if (boolStr.ToUpper().Contains('S'))
            response = true;
        else if (boolStr.ToUpper().Contains('N'))
            response = false;
        return response;
    }
}
