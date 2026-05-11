using ClosedXML.Excel;
using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.Data;
using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EmployeeManagementSystem.Services;

public class TrustControlService
{
    private readonly EmployeeManagementContext _DbContext; // Replace with your actual DbContext
    private CultureInfo _cultureInfo = new CultureInfo("es-MX");
    private const int PageSize = 10;

    //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>CeccResults
    public TrustControlService(EmployeeManagementContext DbContext)
    {
        _DbContext = DbContext;
    }

    /// <summary>
    /// Procesa la información de un archivo Excel para crear nuevos empleados en la base de datos. El archivo debe tener una estructura específica con los campos en el orden correcto, de lo contrario se generarán errores. Si un empleado con el mismo Id ya existe en la base de datos, no se creará nuevamente y se agregará un mensaje de error indicando que el elemento ya existe.
    /// </summary>
    /// <param name="fileStream">Stream del archivo Excel</param>
    /// <returns>Una tupla que contiene el número de empleados importados y una lista de errores encontrados durante la importación</returns>
    public async Task<(int count, List<string> errors)> ImportTrustControlsFromExcelAsync(Stream fileStream)
    {
        var trustControls = new List<CeccResult>();
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
                    var employee = new CeccResult
                    {
                        EmployeeNumber = row.Cell(1).GetValue<int>(),
                        OfficialLetter = !row.Cell(2).IsEmpty() ? row.Cell(4).GetValue<string?>() : null,
                        OfficialLetterDate = !row.Cell(3).IsEmpty() ? getDate(row.Cell(3).GetValue<string>()) : null,
                        IsApproved = !row.Cell(4).IsEmpty() ? getBool(row.Cell(4).GetValue<string>()) : null,
                        IsInforce = !row.Cell(5).IsEmpty() ? getBool(row.Cell(5).GetValue<string>()) : null,
                        ExpirationDate = !row.Cell(6).IsEmpty() ? getDate(row.Cell(6).GetValue<string>()) : null,
                        Notification = !row.Cell(7).IsEmpty() ? row.Cell(7).GetValue<string?>() : null
                    };

                    // Se valida que los campos requeridos sean correctos
                    if (employee.EmployeeNumber == 0)
                        errors.Add($"Fila {row.RowNumber()}: El nombre es requerido.");
                    else
                        trustControls.Add(employee);
                }
                catch (Exception ex)
                {
                    // Si ocurre un error al convertir los datos, se agrega un mensaje de error específico para esa fila
                    errors.Add($"Fila {row.RowNumber()}: Error al convertir los datos. {ex.Message}");
                }
            }

            if (trustControls.Any())
            {
                _DbContext.CeccResults.AddRange(trustControls);
                await _DbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            errors.Add($"ERROR al querer guardar la información en la base de datos: {ex.Message}");
        }
        return (trustControls.Count, errors);
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
    public async Task<int> GetTrustControlsPaginationAsync(int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
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
    public async Task<List<EmployeeDto>> GetTrustControlsAsync(int pageNumber, int? Id = null, string? Name = null, string? Lastname = null, string? Rfc = null, string? Curp = null,
        string? Cuip = null, string? PhoneNumber = null, string? Email = null)
    {
        try
        {
            //Se filtra la información de acuerdo a los parámetros recibidos, si un parámetro es nulo, se ignora en el filtro.
            //Los elementos a obtener se limitan al tamaño de la página y al número de página recibido.
            var query = await _DbContext.Employees
                .AsNoTracking()
                .Include(e => e.CeccResults)
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

            //Se genera la lista de objetos DTO que se enviarán al cliente, solo se incluyen los campos necesarios para mostrar en la tabla
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

    /// <summary>
    /// Ontiene la información completa del empleado de acuerdo al Id recibido, se recomienda usar este método solo para obtener la información de un empleado específico.
    /// </summary>
    /// <param name="Id">Número de empleado</param>
    /// <returns>Objeto Employee con la información completa del empleado</returns>
    public async Task<Employee> GetTrustControlAsync(int Id)
    {
        try
        {
            var res = await _DbContext.Employees
                .Include(e => e.CeccResults)
                .FirstOrDefaultAsync(x => x.Id == Id);
            return res ?? new();
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
    /// <param name="updatedEmployee">El objeto Employee con los datos actualizados.</param>
    /// <returns></returns>
    public async Task UpdateEmployeeAsync(Employee updatedEmployee)
    {
        try
        {
            var employeeFromDb = await _DbContext.Employees
                .FirstOrDefaultAsync(x => x.Id == updatedEmployee.Id);

            if (employeeFromDb is null)
                return;

            employeeFromDb.Id = updatedEmployee.Id;
            employeeFromDb.Name = updatedEmployee.Name;
            employeeFromDb.LastName = updatedEmployee.LastName;
            employeeFromDb.Rfc = updatedEmployee.Rfc;
            employeeFromDb.Curp = updatedEmployee.Curp;
            employeeFromDb.Cuip = updatedEmployee.Cuip;
            employeeFromDb.HighestEducationLevel = updatedEmployee.HighestEducationLevel;
            employeeFromDb.ProfessionalLicense = updatedEmployee.ProfessionalLicense;
            employeeFromDb.ProfessionalLicenseDate = updatedEmployee.ProfessionalLicenseDate;
            employeeFromDb.Gender = updatedEmployee.Gender;
            employeeFromDb.PhoneNumber = updatedEmployee.PhoneNumber;
            employeeFromDb.Email = updatedEmployee.Email;
            employeeFromDb.Address = updatedEmployee.Address;
            employeeFromDb.Neighborhood = updatedEmployee.Neighborhood;
            employeeFromDb.Zip = updatedEmployee.Zip;
            employeeFromDb.City = updatedEmployee.City;
            employeeFromDb.StateId = updatedEmployee.StateId;
            employeeFromDb.PhoneNumber2 = updatedEmployee.PhoneNumber2;
            employeeFromDb.PhoneNumber3 = updatedEmployee.PhoneNumber3;
            employeeFromDb.EmergencyContact = updatedEmployee.EmergencyContact;
            employeeFromDb.Birthdate = updatedEmployee.Birthdate;
            employeeFromDb.Height = updatedEmployee.Height;
            employeeFromDb.MaritalStatusId = updatedEmployee.MaritalStatusId;
            employeeFromDb.BirthPlace = updatedEmployee.BirthPlace;
            employeeFromDb.ChildrenNumber = updatedEmployee.ChildrenNumber;
            employeeFromDb.DriverLicense = updatedEmployee.DriverLicense;
            employeeFromDb.LicenseType = updatedEmployee.LicenseType;
            employeeFromDb.LicenseIssuedStateId = updatedEmployee.LicenseIssuedStateId;
            employeeFromDb.LicenseIssuedDate = updatedEmployee.LicenseIssuedDate;
            employeeFromDb.LicenseExpirationDate = updatedEmployee.LicenseExpirationDate;
            employeeFromDb.IsPermanentLicense = updatedEmployee.IsPermanentLicense;

            _DbContext.Employees.Update(employeeFromDb);
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
