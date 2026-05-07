using EmployeeManagementSystem.Components.Modals;
using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;

namespace EmployeeManagementSystem.Components.Pages;

public partial class EmployeeQueries
{
    private readonly IEmployeesService _employeesService;
    private List<EmployeeDto> _Employees = new List<EmployeeDto>();
    private bool _isLoading = false;
    private int _totalPages = 1;
    private int _currentPage = 1;

    private int? _employeeNumber = null;
    private string _name = null;
    private string _lastname = null;
    private string _rfc = null;
    private string _cuip = null;

    private int? _staticEmployeeNumber;
    private string _staticName = null;
    private string _staticLastname = null;
    private string _staticRfc = null;
    private string _staticCuip = null;

    private EmployeeInformationModal _modal = default!;

    public EmployeeQueries(IEmployeesService employeesService)
    {
        _employeesService = employeesService;
    }

    private async Task FilterEmployees()
    {
        _currentPage = 1;
        _isLoading = true;
        StateHasChanged();

        await Task.Delay(200);
        _staticEmployeeNumber = _employeeNumber;
        _staticName = _name;
        _staticLastname = _lastname;
        _staticRfc = _rfc;
        _staticCuip = _cuip;
        _totalPages = await _employeesService.GetEmployeesPaginationAsync(_staticEmployeeNumber, _staticName, _staticLastname, _staticRfc, null, _staticCuip);
        _Employees = await _employeesService.GetEmployeesAsync(1, _staticEmployeeNumber, _staticName, _staticLastname, _staticRfc, null, _staticCuip);

        _isLoading = false;
        StateHasChanged();
    }

    private async Task ChangePage(int pageId)
    {
        _currentPage = pageId;
        _isLoading = true;
        StateHasChanged();

        await Task.Delay(200);
        _Employees = await _employeesService.GetEmployeesAsync(pageId, _staticEmployeeNumber, _staticName, _staticLastname, _staticRfc, null, _staticCuip);

        _isLoading = false;
        StateHasChanged();
    }

    private async Task OpenModal(int employeeId)
    {
        try
        {
            await _modal.Open(employeeId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
        }
    }

    private async Task HandleSave(Employee updated)
    {
        // Find the record in the local list and update it
        var idx = _Employees.FindIndex(e => e.Id == updated.Id);
        if (idx >= 0)
        {
            _Employees[idx] = new EmployeeDto
            {
                Id = updated.Id,
                Name = updated.Name,
                LastName = updated.LastName,
                Rfc = updated.Rfc ?? "",
                Cuip = updated.Cuip ?? "",
                PhoneNumber = updated.PhoneNumber ?? "",
                Email = updated.Email ?? ""
            };
        }            

        // Persist to your backend here, e.g.:
        await _employeesService.UpdateEmployeeAsync(updated);         
    }
}
