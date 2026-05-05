using ClosedXML.Excel;
using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.Data;
using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EmployeeManagementSystem.Services;

public class EmployeesService : IEmployeesService
{
    private readonly EmployeeManagementContext _DbContext; // Replace with your actual DbContext
    private CultureInfo _cultureInfo = new CultureInfo("es-MX");
    private const int PageSize = 2;

    public EmployeesService(EmployeeManagementContext DbContext)
    {
        _DbContext = DbContext;
    }

    public async Task<(int count, List<string> errors)> ImportUsersFromExcelAsync(Stream fileStream)
    {
        var employees = new List<Employee>();
        var errors = new List<string>();
        var validEmployees = new List<Employee>();

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
                    var employee = new Employee
                    {
                        Id =                        row.Cell(1).GetValue<int>(),
                        Name =                      row.Cell(2).GetValue<string>(),
                        LastName =                  row.Cell(3).GetValue<string>(),
                        Rfc =                       !row.Cell(4).IsEmpty() ? row.Cell(4).GetValue<string?>() : null,
                        Curp =                      !row.Cell(5).IsEmpty() ? row.Cell(5).GetValue<string?>() : null,
                        Cuip =                      !row.Cell(6).IsEmpty() ? row.Cell(6).GetValue<string?>() : null,
                        HighestEducationLevel =     !row.Cell(7).IsEmpty() ? row.Cell(7).GetValue<string?>() : null,
                        ProfessionalLicense =       !row.Cell(8).IsEmpty() ? row.Cell(8).GetValue<string?>() : null,
                        ProfessionalLicenseDate =   !row.Cell(9).IsEmpty() ? getDate(row.Cell(9).GetValue<string>()) : null,
                        Gender =                    !row.Cell(10).IsEmpty() ? row.Cell(10).GetValue<string?>() : null,
                        PhoneNumber =               !row.Cell(11).IsEmpty() ? row.Cell(11).GetValue<string?>() : null,
                        Email =                     !row.Cell(12).IsEmpty() ? row.Cell(12).GetValue<string?>() : null,
                        Address =                   !row.Cell(13).IsEmpty() ? row.Cell(13).GetValue<string?>() : null,
                        Neighborhood =              !row.Cell(14).IsEmpty() ? row.Cell(14).GetValue<string?>() : null,
                        Zip =                       !row.Cell(15).IsEmpty() ? row.Cell(15).GetValue<string?>() : null,
                        City =                      !row.Cell(16).IsEmpty() ? row.Cell(16).GetValue<string?>() : null,
                        StateId =                   (!row.Cell(18).IsEmpty() && row.Cell(18).GetValue<int>() != 0) ? row.Cell(18).GetValue<int?>() : null,//Foreignkey
                        PhoneNumber2 =              !row.Cell(19).IsEmpty() ? row.Cell(19).GetValue<string?>() : null,
                        PhoneNumber3 =              !row.Cell(20).IsEmpty() ? row.Cell(20).GetValue<string?>() : null,
                        EmergencyContact =          !row.Cell(21).IsEmpty() ? row.Cell(21).GetValue<string?>() : null,
                        Birthdate =                 !row.Cell(22).IsEmpty() ? getDate(row.Cell(22).GetValue<string>()) : null,
                        Height =                    !row.Cell(23).IsEmpty() ? row.Cell(23).GetValue<int?>() : null,
                        MaritalStatusId =           (!row.Cell(25).IsEmpty() && row.Cell(25).GetValue<int>() != 0) ? row.Cell(25).GetValue<int?>() : null,//Foreignkey
                        BirthPlace =                !row.Cell(26).IsEmpty() ? row.Cell(26).GetValue<string?>() : null,
                        ChildrenNumber =            !row.Cell(27).IsEmpty() ? row.Cell(27).GetValue<int?>() : null,
                        DriverLicense =             !row.Cell(28).IsEmpty() ? row.Cell(28).GetValue<string?>() : null,
                        LicenseType =               !row.Cell(29).IsEmpty() ? row.Cell(29).GetValue<string?>() : null,
                        LicenseIssuedStateId =      (!row.Cell(31).IsEmpty() && row.Cell(31).GetValue<int>() != 0) ? row.Cell(31).GetValue<int?>() : null,//Foreignkey
                        LicenseIssuedDate =         !row.Cell(32).IsEmpty() ? getDate(row.Cell(32).GetValue<string>()) : null,
                        LicenseExpirationDate =     !row.Cell(33).IsEmpty() ? getDate(row.Cell(33).GetValue<string>()) : null,
                        IsPermanentLicense =        !row.Cell(34).IsEmpty() ? getBool(row.Cell(34).GetValue<string>()) : null
                    };

                    // Basic validation
                    if (string.IsNullOrWhiteSpace(employee.Name))
                        errors.Add($"Fila {row.RowNumber()}: El nombre es requerido.");
                    else if (string.IsNullOrWhiteSpace(employee.LastName))
                        errors.Add($"Fila {row.RowNumber()}: El apellido es requerido.");
                    else
                        employees.Add(employee);
                }
                catch (Exception ex)
                {
                    errors.Add($"Fila {row.RowNumber()}: Error al convertir los datos. {ex.Message}");
                }
            }

            if (employees.Any())
            {
                var incomingIds = employees.Select(x => x.Id).ToList();
                //Lista de Ids que ya existen en la BD
                var existingIds = await _DbContext.Employees.Where(e => incomingIds.Contains(e.Id))
                    .Select(e => e.Id)
                    .ToListAsync();

                foreach (var emp in employees)
                {
                    //Si el Id actual ya existe, se agrega a la lista de errores, si no se crerá el elemento en la BD
                    if (existingIds.Contains(emp.Id))
                    {
                        errors.Add($"El elemento con  Id {emp.Id} ya existe y no será creado nuevamente");
                    }
                    else
                    {
                        validEmployees.Add(emp);
                    }
                }

                if (validEmployees.Any())
                {
                    _DbContext.Employees.AddRange(validEmployees);
                    await _DbContext.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
        }
        return (validEmployees.Count, errors);
    }

    public async Task<int> GetEmployeesPaginationAsync(int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null)
    {
        try
        {
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

            bool areThereRemainingElements = (query.Count() % PageSize) >= 1;
            int totalPages = query.Count() / PageSize;
            totalPages = areThereRemainingElements ? totalPages + 1: totalPages;

            return totalPages;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            return 1;
        }
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync(int pageNumber, int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null)
    {
        try
        {
            var query = await _DbContext.Employees
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
                    )
                .Skip((pageNumber - 1) * PageSize)//Salta la cantidad de elementos calculada
                .Take(PageSize)
                .ToListAsync();

            var result = new List<EmployeeDto>();
            foreach (var employee in query)
            {
                var newEmployee = new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    LastName = employee.LastName,
                    Rfc = employee.Rfc ?? "",
                    Cuip = employee.Cuip ?? "",
                    PhoneNumber = employee.PhoneNumber ?? "",
                    Email = employee.Email ?? ""
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
