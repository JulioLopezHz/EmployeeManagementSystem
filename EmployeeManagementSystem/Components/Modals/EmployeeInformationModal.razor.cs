using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace EmployeeManagementSystem.Components.Modals;

public partial class EmployeeInformationModal
{
    [Parameter] public EventCallback<Employee> OnSave { get; set; }
    [Inject] IEmployeesService _employeesService { get; set; }

    private bool _isVisible = false;
    private Employee _editEmployee = new();
    private bool _isLoading = false;

    private Dictionary<int, string> _States = new Dictionary<int, string>
    {
        { 1, "Ciudad de México" },
        { 2, "Aguascalientes" },
        { 3, "Baja California" },
        { 4, "Baja California Sur" },
        { 5, "Campeche" },
        { 6, "Chiapas" },
        { 7, "Chihuahua" },
        { 8, "Coahuila" },
        { 9, "Colima" },
        { 10, "Durango" },
        { 11, "Estado de México" },
        { 12, "Guanajuato" },
        { 13, "Guerrero" },
        { 14, "Hidalgo" },
        { 15, "Jalisco" },
        { 16, "Michoacán" },
        { 17, "Morelos" },
        { 18, "Nayarit" },
        { 19, "Nuevo León" },
        { 20, "Oaxaca" },
        { 21, "Puebla" },
        { 22, "Querétaro" },
        { 23, "Quintana Roo" },
        { 24, "San Luis Potosí" },
        { 25, "Sinaloa" },
        { 26, "Sonora" },
        { 27, "Tabasco" },
        { 28, "Tamaulipas" },
        { 29, "Tlaxcala" },
        { 30, "Veracruz" },
        { 31, "Yucatán" },
        { 32, "Zacatecas" }
    };

    private Dictionary<int, string> _MaritalStatuses = new Dictionary<int, string>
    {
        { 1, "Soltero"},
        { 2, "Casado"},
        { 3, "Divorciado"},
        { 4, "Viudo"},
        { 5, "Concubinato"}
    };

    private Dictionary<string, string> _Genders = new Dictionary<string, string>
    {
        { "H", "Hombre" },
        { "M", "Mujer" }
    };

    public async Task Open(int employeeId)
    {
        // Clone so edits don't affect the table until saved
        _editEmployee = await _employeesService.GetEmployeeAsync(employeeId);
        _isVisible = true;
        StateHasChanged();
    }

    private void Close()
    {
        _isVisible = false;
    }

    private async Task Save()
    {
        _isLoading = true;
        StateHasChanged();
        await Task.Delay(200);
        try
        {
            await OnSave.InvokeAsync(_editEmployee);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
            _isVisible = false;
        }        
    }

    void OnStatusChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString();
        if (string.IsNullOrEmpty(val))
            _editEmployee.IsPermanentLicense = null;
        else
            _editEmployee.IsPermanentLicense = bool.Parse(val);
    }
}
