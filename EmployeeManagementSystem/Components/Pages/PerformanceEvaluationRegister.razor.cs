using EmployeeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace EmployeeManagementSystem.Components.Pages;

public partial class PerformanceEvaluationRegister
{
    private readonly IPerformanceEvaluationService _PerformanceEvaluationService;
    private IBrowserFile? _selectedFile;
    private string _fileInfo = string.Empty;
    private List<string> _validationErrors = new();
    private string? _successMessage;
    private string? _errorMessage;
    private bool _fileReady;
    private bool _isProcessing;

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public PerformanceEvaluationRegister(IPerformanceEvaluationService performanceEvaluationService)
    {
        _PerformanceEvaluationService = performanceEvaluationService;
    }

    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        Reset();
        _selectedFile = e.File;

        var ext = Path.GetExtension(_selectedFile.Name).ToLowerInvariant();
        if (ext is not ".xlsx" and not ".xls")
        {
            _errorMessage = "Sólo se pueden procesar archivos .xlsx y .xls.";
            return;
        }

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(_selectedFile.Name);
        if (!fileNameWithoutExt.Equals("Evaluación_Desempeño", StringComparison.OrdinalIgnoreCase))
        {
            _errorMessage = "Solo se acepta el archivo llamado 'Evaluación_Desempeño' (ejemplo: Evaluación_Desempeño.xlsx).";
            return;
        }


        if (_selectedFile.Size > MaxFileSizeBytes)
        {
            _errorMessage = $"El archivo escede el límite de 10 MB ({_selectedFile.Size / 1024.0 / 1024.0:F2} MB).";
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

            using var stream = _selectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            var result = await _PerformanceEvaluationService.ImportPerformanceEvaluationFromExcelAsync(ms);

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
