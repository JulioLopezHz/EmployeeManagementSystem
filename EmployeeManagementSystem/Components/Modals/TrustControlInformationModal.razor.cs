using EmployeeManagementSystem.Interfaces;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EmployeeManagementSystem.Components.Modals;

public partial class TrustControlInformationModal
{
    [Parameter] public EventCallback<Employee> OnSave { get; set; }
    [Inject] ITrustControlService _TrustControlService { get; set; }
    [Inject] PdfService _PdfService { get; set; }
    [Inject] IJSRuntime _JS { get; set; }

    private bool _isVisible = false;
    private Employee _editTrustControl = new();
    private bool _isLoading = false;

    private Dictionary<int, string> _ProfessionalCareerServices = new Dictionary<int, string>
    {
        { 1, "Incorporación examen"},
        { 2, "Ingreso por convocatoria"},
        { 3, "Tránsito examen"}
    };

    public async Task Open(int employeeId)
    {
        // Clone so edits don't affect the table until saved
        _editTrustControl = await _TrustControlService.GetTrustControlAsync(employeeId);
        _isVisible = true;
        StateHasChanged();
    }

    private void Close()
    {
        _isVisible = false;
    }
    
    private async Task Save()
    {
        await OnSave.InvokeAsync(_editTrustControl);
        _isVisible = false;
    }

    void OnIsApprovedChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString();
        if (string.IsNullOrEmpty(val))
            _editTrustControl.CeccResults.LastOrDefault()?.IsApproved = null;
        else
            _editTrustControl.CeccResults.LastOrDefault()?.IsApproved = bool.Parse(val);
    }

    void OnIsInforceChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString();
        if (string.IsNullOrEmpty(val))
            _editTrustControl.CeccResults.LastOrDefault()?.IsInforce = null;
        else
            _editTrustControl.CeccResults.LastOrDefault()?.IsInforce = bool.Parse(val);
    }

    void OnIsDirectDesignationChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString();
        if (string.IsNullOrEmpty(val))
            _editTrustControl.PayrollRecord?.IsDirectDesignation = null;
        else
            _editTrustControl.PayrollRecord?.IsDirectDesignation = bool.Parse(val);
    }

    private async Task DownloadPdf()
    {
        _isLoading = true;
        StateHasChanged();
        await Task.Delay(200);
        try
        {
            var bytes = await _PdfService.GenerateTrustControlPdf(_editTrustControl);
            string base64 = Convert.ToBase64String(bytes);
            string fileName = $"Control y Confianza Empleado_{_editTrustControl.Id}.pdf";
            await _JS.InvokeVoidAsync("downloadFile", fileName, base64);
            //await _JS.InvokeVoidAsync("openPdfInNewTab", base64);
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
