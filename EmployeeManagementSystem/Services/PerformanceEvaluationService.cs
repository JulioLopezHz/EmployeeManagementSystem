using ClosedXML.Excel;
using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.Data;
using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EmployeeManagementSystem.Services;

public class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly EmployeeManagementContext _DbContext;
    private CultureInfo _cultureInfo = new CultureInfo("es-MX");
    private const int PageSize = 10;

    public PerformanceEvaluationService(EmployeeManagementContext DbContext)
    {
        _DbContext = DbContext;
    }

    /// <summary>
    /// Procesa la información de un archivo Excel para crear nuevos empleados en la base de datos. El archivo debe tener una estructura específica con los campos en el orden correcto, de lo contrario se generarán errores. Si un empleado con el mismo Id ya existe en la base de datos, no se creará nuevamente y se agregará un mensaje de error indicando que el elemento ya existe.
    /// </summary>
    /// <param name="fileStream">Stream del archivo Excel</param>
    /// <returns>Una tupla que contiene el número de empleados importados y una lista de errores encontrados durante la importación</returns>
    public async Task<(int count, List<string> errors)> ImportPerformanceEvaluationFromExcelAsync(Stream fileStream)
    {
        var PerformanceEvaluations = new List<PerformanceEvaluation>();
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
                    var expirationDate = !row.Cell(6).IsEmpty() ? getDate(row.Cell(6).GetValue<string>()) : null;
                    var employeeNumber = row.Cell(1).GetValue<int>();

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

                    var performanceEvaluation = new PerformanceEvaluation
                    {
                        EmployeeNumber = row.Cell(1).GetValue<int>(),
                        FinalTestScore = !row.Cell(2).IsEmpty() ? row.Cell(2).GetValue<decimal>() : 0.0m,
                        PerformanceLevelId = !row.Cell(4).IsEmpty() ? row.Cell(4).GetValue<int>() : 3,
                        AcknowledgementDate = !row.Cell(5).IsEmpty() ? getDate(row.Cell(5).GetValue<string>()) : null,
                        UnitDeliveryDate = !row.Cell(6).IsEmpty() ? getDate(row.Cell(6).GetValue<string>()) : null,
                        DeliveryLetter = !row.Cell(7).IsEmpty() ? row.Cell(7).GetValue<string>() : null,
                        Observations = !row.Cell(8).IsEmpty() ? row.Cell(8).GetValue<string>() : null,
                        IndividualActionPlan = !row.Cell(9).IsEmpty() ? row.Cell(9).GetValue<string>() : null
                    };

                    PerformanceEvaluations.Add(performanceEvaluation);
                }
                catch (Exception ex)
                {
                    // Si ocurre un error al convertir los datos, se agrega un mensaje de error específico para esa fila
                    errors.Add($"Fila {row.RowNumber()}: Error al convertir los datos. {ex.Message}");
                }
            }

            if (PerformanceEvaluations.Any())
            {
                _DbContext.PerformanceEvaluations.AddRange(PerformanceEvaluations);
                await _DbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            errors.Add($"ERROR al querer guardar la información en la base de datos: {ex.Message}");
        }
        return (PerformanceEvaluations.Count, errors);
    }

    /// <summary>
    /// Calcula el número total de páginas necesarias para mostrar los empleados de acuerdo a los filtros recibidos y al tamaño de página definido.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="Lastname"></param>
    /// <param name="Rfc"></param>
    /// <param name="Curp"></param>
    /// <param name="Cuip"></param>
    /// <param name="PhoneNumber"></param>
    /// <param name="Email"></param>
    /// <returns>El total de páginas calculadas</returns>
    public async Task<int> GetPerformanceEvaluationPaginationAsync(int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null)
    {
        try
        {
            //Se filtra la información de acuerdo a los parámetros recibidos, si un parámetro es nulo, se ignora en el filtro
            var query = _DbContext.Employees
                .AsNoTracking()
                .Where(e =>
                (Id == null || e.Id == Id)
                && (Name == null || e.Name.Contains(Name))
                && (Lastname == null || e.LastName.Contains(Lastname))
                && (Rfc == null || (e.Rfc != null && e.Rfc.Contains(Rfc)))
                && (Curp == null || (e.Curp != null && e.Curp.Contains(Curp)))
                && (Cuip == null || (e.Cuip != null && e.Cuip.Contains(Cuip)))
                && (PhoneNumber == null || (e.PhoneNumber != null && e.PhoneNumber.Contains(PhoneNumber)))
                && (Email == null || (e.Email != null && e.Email.Contains(Email)))
                );

            //Si sobran elementos sobre el tamaño de la página, se agrega una página adicional para mostrar el resto
            bool areThereRemainingElements = (query.Count() % PageSize) >= 1;
            int totalPages = query.Count() / PageSize;
            totalPages = areThereRemainingElements ? totalPages + 1 : totalPages;

            return totalPages;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            return 1;
        }
    }

    /// <summary>
    /// Obtiene la lista de elementos de acuerdo a los filtros recibidos y al número de página solicitado.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="Lastname"></param>
    /// <param name="Rfc"></param>
    /// <param name="Curp"></param>
    /// <param name="Cuip"></param>
    /// <param name="PhoneNumber"></param>
    /// <param name="Email"></param>
    /// <returns>Lista de DTOs</returns>
    public async Task<List<PerformanceEvaluationDto>> GetPerformanceEvaluationsAsync(int pageNumber, int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null)
    {
        try
        {
            //Se filtra la información de acuerdo a los parámetros recibidos, si un parámetro es nulo, se ignora en el filtro.
            //Los elementos a obtener se limitan al tamaño de la página y al número de página recibido.
            var query = await _DbContext.Employees
                .AsNoTracking()
                .Include(e => e.PerformanceEvaluations)
                .ThenInclude(pe => pe.PerformanceLevel)
                .Where(e =>
                    (Id == null || e.Id == Id)
                    && (Name == null || e.Name.Contains(Name))
                    && (Lastname == null || e.LastName.Contains(Lastname))
                    && (Rfc == null || (e.Rfc != null && e.Rfc.Contains(Rfc)))
                    && (Curp == null || (e.Curp != null && e.Curp.Contains(Curp)))
                    && (Cuip == null || (e.Cuip != null && e.Cuip.Contains(Cuip)))
                    && (PhoneNumber == null || (e.PhoneNumber != null && e.PhoneNumber.Contains(PhoneNumber)))
                    )
                .Skip((pageNumber - 1) * PageSize)//Salta la cantidad de elementos calculada
                .Take(PageSize)
                .ToListAsync();

            //Se genera la lista de objetos DTO que se enviarán al cliente, solo se incluyen los campos necesarios para mostrar en la tabla
            var result = new List<PerformanceEvaluationDto>();
            foreach (var employee in query)
            {
                DateOnly? expirationDate = employee.CeccResults?.LastOrDefault()?.ExpirationDate;
                var newEmployee = new PerformanceEvaluationDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    LastName = employee.LastName,
                    Rfc = employee.Rfc ?? "",
                    Curp = employee.Curp ?? "",
                    Cuip = employee.Cuip ?? "",
                    PhoneNumber = employee.PhoneNumber ?? "",
                    PerformanceLevel = employee.PerformanceEvaluations?.LastOrDefault()?.PerformanceLevel?.Name ?? "Sin evaluación"
                };
                result.Add(newEmployee);
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            return new();
        }
    }

    /// <summary>
    /// Ontiene la información completa del empleado de acuerdo al Id recibido, se recomienda usar este método solo para obtener la información de un empleado específico.
    /// </summary>
    /// <param name="Id">Número de empleado</param>
    /// <returns>Objeto Employee con la información completa del empleado</returns>
    public async Task<PerformanceEvaluationModalDto> GetPerformanceEvaluationAsync(int Id)
    {
        try
        {
            var res = await _DbContext.Employees
                .AsNoTracking()
                .Where(e => e.Id == Id)
                .Select(e => new PerformanceEvaluationModalDto
                {
                    EmployeeNumber = e.Id,
                    Name = e.Name,
                    LastName = e.LastName,
                    LastPayrollPosition = e.PayrollRecord.PayrollPositionRecords.OrderBy(p => p.CreationDate).LastOrDefault().PayrollPosition.Name,
                    Rfc = e.Rfc ?? "",
                    Curp = e.Curp ?? "",
                    Cuip = e.Cuip ?? "",
                    PhoneNumber = e.PhoneNumber ?? "",
                    FinalTestScore = e.PerformanceEvaluations.OrderBy(pe => pe.AcknowledgementDate).Last().FinalTestScore,
                    PerformanceLevelId = e.PerformanceEvaluations.OrderBy(pe => pe.AcknowledgementDate).Last().PerformanceLevelId,
                    AcknowledgementDate = e.PerformanceEvaluations.OrderBy(pe => pe.AcknowledgementDate).Last().AcknowledgementDate,
                    UnitDeliveryDate = e.PerformanceEvaluations.OrderBy(pe => pe.AcknowledgementDate).Last().UnitDeliveryDate,
                    DeliveryLetter = e.PerformanceEvaluations.OrderBy(pe => pe.AcknowledgementDate).Last().DeliveryLetter,
                    Observations = e.PerformanceEvaluations.OrderBy(pe => pe.AcknowledgementDate).Last().Observations,
                    IndividualActionPlan = e.PerformanceEvaluations.OrderBy(pe => pe.AcknowledgementDate).Last().IndividualActionPlan
                })
                .FirstOrDefaultAsync() ?? new PerformanceEvaluationModalDto();
            return res;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            return new();
        }
    }

    /// <summary>
    /// Actualiza todas las propiedades del empleado, se recomienda enviar el objeto completo con las propiedades que no se quieran actualizar con su valor actual para evitar perder información.
    /// </summary>
    /// <param name="updatedPerformanceEvaluation">El objeto Employee con los datos actualizados.</param>
    /// <returns></returns>
    public async Task UpdatePerformanceEvaluationAsync(PerformanceEvaluationModalDto updatedPerformanceEvaluation)
    {
        try
        {
            var employeeFromDb = await _DbContext.Employees
                .FirstOrDefaultAsync(x => x.Id == updatedPerformanceEvaluation.EmployeeNumber);

            if (employeeFromDb is null)
                return;

            employeeFromDb.Id = updatedPerformanceEvaluation.EmployeeNumber;
            employeeFromDb.Name = updatedPerformanceEvaluation.Name;
            employeeFromDb.LastName = updatedPerformanceEvaluation.LastName;
            employeeFromDb.Rfc = updatedPerformanceEvaluation.Rfc;
            employeeFromDb.Curp = updatedPerformanceEvaluation.Curp;
            employeeFromDb.Cuip = updatedPerformanceEvaluation.Cuip;
            employeeFromDb.PhoneNumber = updatedPerformanceEvaluation.PhoneNumber;

            var evaluation = await _DbContext.PerformanceEvaluations
                .Where(pe => pe.EmployeeNumber == updatedPerformanceEvaluation.EmployeeNumber)
                .OrderBy(pe => pe.AcknowledgementDate)
                .LastOrDefaultAsync();

            if (evaluation is null)
                return;

            evaluation.FinalTestScore = updatedPerformanceEvaluation.FinalTestScore;
            evaluation.PerformanceLevelId = updatedPerformanceEvaluation.PerformanceLevelId;
            evaluation.AcknowledgementDate = updatedPerformanceEvaluation.AcknowledgementDate;
            evaluation.UnitDeliveryDate = updatedPerformanceEvaluation.UnitDeliveryDate;
            evaluation.DeliveryLetter = updatedPerformanceEvaluation.DeliveryLetter;
            evaluation.Observations = updatedPerformanceEvaluation.Observations;
            evaluation.IndividualActionPlan = updatedPerformanceEvaluation.IndividualActionPlan;

            //var position = await _DbContext.PayrollPositionRecords
            //    .Where(ppr => ppr.EmployeeNumber == updatedPerformanceEvaluation.EmployeeNumber)
            //    .OrderBy(ppr => ppr.CreationDate)
            //    .LastOrDefaultAsync();

            await _DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
        }
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
