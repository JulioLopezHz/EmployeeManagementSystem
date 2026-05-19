using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EmployeeManagementSystem.Components.Modals;

public partial class PerformanceEvaluationModal
{
    [Parameter] public EventCallback<PerformanceEvaluationModalDto> OnSave { get; set; }
    [Inject] IPerformanceEvaluationService _PerformanceEvaluationService { get; set; }
    [Inject] PdfService _PdfService { get; set; }
    [Inject] IJSRuntime _JS { get; set; }

    private bool _isVisible = false;
    private PerformanceEvaluationModalDto _editPerformanceEvaluation = new();
    private bool _isLoading = false;

    private Dictionary<int, string> _PerformanceLevels = new Dictionary<int, string>
    {
        { 1, "Muy bueno"},
        { 2, "Satisfactorio"},
        { 3, "Por mejorar"}
    };

    public async Task Open(int employeeId)
    {
        // Clone so edits don't affect the table until saved
        _editPerformanceEvaluation = await _PerformanceEvaluationService.GetPerformanceEvaluationAsync(employeeId);
        _isVisible = true;
        StateHasChanged();
    }

    private void Close()
    {
        _isVisible = false;
    }

    private async Task Save()
    {
        await OnSave.InvokeAsync(_editPerformanceEvaluation);
        _isVisible = false;
    }

    private async Task DownloadPdf()
    {
        _isLoading = true;
        StateHasChanged();
        await Task.Delay(200);
        try
        {
            var bytes = await _PdfService.GeneratePerformanceEvaluationPdf(_editPerformanceEvaluation);
            string base64 = Convert.ToBase64String(bytes);
            string fileName = $"Control y Confianza Empleado_{_editPerformanceEvaluation.PerformanceLevelId}.pdf";
            await _JS.InvokeVoidAsync("downloadFile", fileName, base64);
            await _JS.InvokeVoidAsync("openPdfInNewTab", base64);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}
