using EmployeeManagementSystem.Components.Modals;
using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;

namespace EmployeeManagementSystem.Components.Pages;

public partial class PerformanceEvaluationQueries
{
    private readonly IPerformanceEvaluationService _PerformanceEvaluationService;
    private List<PerformanceEvaluationDto> _performanceEvaluations = new List<PerformanceEvaluationDto>();
    private bool _isLoading = false;
    private int _totalPages = 1;
    private int _currentPage = 1;

    private int? _employeeNumber = null;
    private string _name = null;
    private string _lastname = null;
    private string _rfc = null;
    private string _curp = null;
    private string _cuip = null;

    private int? _staticEmployeeNumber;
    private string _staticName = null;
    private string _staticLastname = null;
    private string _staticRfc = null;
    private string _staticCurp = null;
    private string _staticCuip = null;

    private PerformanceEvaluationModal _modal = default!;

    public PerformanceEvaluationQueries(IPerformanceEvaluationService performanceEvaluationService)
    {
        _PerformanceEvaluationService = performanceEvaluationService;
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
        _staticCurp= _curp;
        _staticCuip = _cuip;
        _totalPages = await _PerformanceEvaluationService.GetPerformanceEvaluationPaginationAsync(_staticEmployeeNumber, _staticName, _staticLastname, _staticRfc, null, _staticCuip);
        _performanceEvaluations = await _PerformanceEvaluationService.GetPerformanceEvaluationsAsync(1, _staticEmployeeNumber, _staticName, _staticLastname, _staticRfc, _staticCurp, _staticCuip, null);

        _isLoading = false;
        StateHasChanged();
    }

    private IEnumerable<int> GetPageNumbers()
    {
        // Muestra todas las páginas si el total es 10 o menos
        if (_totalPages <= 10)
        {
            return Enumerable.Range(1, _totalPages);
        }

        // Muestra las primeras 9 páginas + puntos suspensivos + última página
        // -1 se usa como un valor centinela para renderizar la elipsis
        var pages = new List<int>();

        if (_currentPage <= 9)
        {
            pages.AddRange(Enumerable.Range(1, 9)); // Páginas 1–9
            pages.Add(-1);                          // puntos suspensivos
            pages.Add(_totalPages);                 // Última página
        }
        else if (_currentPage >= _totalPages - 1)
        {
            pages.Add(1);                                                      // Primera página
            pages.Add(-1);                                                     // puntos suspensivos
            pages.AddRange(Enumerable.Range(_totalPages - 8, 9));              // Últimas 9 páginas
        }
        else
        {
            pages.Add(1);                                         // Primera página
            pages.Add(-1);                                        // puntos suspensivos
            pages.AddRange(Enumerable.Range(_currentPage - 2, 5)); // Ventana: 2 antes → actual → 2 después
            pages.Add(-1);                                        // puntos suspensivos
            pages.Add(_totalPages);                               // Última página
        }

        return pages;
    }

    private async Task ChangePage(int pageId)
    {
        _currentPage = pageId;
        _isLoading = true;
        StateHasChanged();

        await Task.Delay(200);
        _performanceEvaluations = await _PerformanceEvaluationService.GetPerformanceEvaluationsAsync(_currentPage, _staticEmployeeNumber, _staticName, _staticLastname, _staticRfc, _staticCurp, _staticCuip, null);

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

    private async Task HandleSave(PerformanceEvaluationModalDto updated)
    {
        // Find the record in the local list and update it
        var idx = _performanceEvaluations.FindIndex(e => e.Id == updated.EmployeeNumber);
        //if (idx >= 0)
        //{
        //    _Employees[idx] = new EmployeeDto
        //    {
        //        Id = updated.Id,
        //        Name = updated.Name,
        //        LastName = updated.LastName,
        //        Rfc = updated.Rfc ?? "",
        //        Cuip = updated.Cuip ?? "",
        //        PhoneNumber = updated.PhoneNumber ?? "",
        //        Email = updated.Email ?? ""
        //    };
        //}

        //// Persist to your backend here, e.g.:
        await _PerformanceEvaluationService.UpdatePerformanceEvaluationAsync(updated);
    }
}
