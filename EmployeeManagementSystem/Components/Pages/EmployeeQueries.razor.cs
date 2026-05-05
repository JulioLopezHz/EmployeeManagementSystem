using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.DTOs;

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
}
