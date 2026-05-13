using EmployeeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace EmployeeManagementSystem.Components.Pages;

public partial class EmployeesRegister
{
    private readonly IEmployeesService _employeesService;
    private IBrowserFile? _selectedFile;
    private string _fileInfo = string.Empty;
    private List<string> _validationErrors = new();
    private string? _successMessage;
    private string? _errorMessage;
    private bool _fileReady;
    private bool _isProcessing;

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public EmployeesRegister(IEmployeesService employeesImportService)
    {
        _employeesService = employeesImportService;
    }

    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        Reset();
        _selectedFile = e.File;

        var ext = Path.GetExtension(_selectedFile.Name).ToLowerInvariant();
        if (ext is not ".xlsx" and not ".xls")
        {
            _errorMessage = "Only .xlsx and .xls files are supported.";
            return;
        }

        if (_selectedFile.Size > MaxFileSizeBytes)
        {
            _errorMessage = $"File exceeds the 10 MB limit ({_selectedFile.Size / 1024.0 / 1024.0:F2} MB).";
            return;
        }

        _fileReady = true;
        _fileInfo = BuildFileInfo(_selectedFile);
    }

    private async Task SubmitAsync()
    {
        if (_selectedFile == null) return;

        try
        {
            _isProcessing = true;
            _validationErrors.Clear();

            // OpenReadStream has a default max size of 512KB. 
            // Adjust maxAllowedSize if your Excel files are large.
            using var stream = _selectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            var result = await _employeesService.ImportUsersFromExcelAsync(ms);

            if (result.count > 0 && !result.errors.Any(e => e.Contains("base de datos")))
                _successMessage = $"Se importaron {result.count} usuarios correctamente.";

            _validationErrors = result.errors;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Ocurrió un error crítico: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private static string BuildFileInfo(IBrowserFile file)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Nombre      : {file.Name}");
        sb.AppendLine($"Tamaño      : {file.Size / 1024.0:F1} KB ({file.Size:N0} bytes)");
        // sb.AppendLine($"Type      : {file.ContentType}");
        sb.AppendLine($"Última mod. : {file.LastModified:yyyy-MM-dd HH:mm:ss}");
        return sb.ToString().TrimEnd();
    }

    private void Reset()
    {
        _selectedFile = null;
        _fileInfo = string.Empty;
        _fileReady = false;
        _successMessage = null;
        _errorMessage = null;
        _validationErrors = new();
    }
}
