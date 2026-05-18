using EmployeeManagementSystem.Shared.DTOs;
using EmployeeManagementSystem.Shared.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using System.Globalization;

namespace EmployeeManagementSystem.Services;

public class PdfService
{
    private readonly IWebHostEnvironment _env;
    CultureInfo culture = new CultureInfo("es-MX");

    public PdfService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<byte[]> GenerateTrustControlPdf(Employee employee)
    {
        try
        {
            using (var document = new PdfDocument())
            {
                document.Info.Title = "Resultado de Control y Confianza";
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                string logoPath = Path.Combine(_env.WebRootPath, "Images", "Logo_Dependencia_inv.png");
                if (File.Exists(logoPath))
                {
                    // Load the image
                    using (XImage logo = XImage.FromFile(logoPath))
                    {
                        // Parameters: Image, X, Y, Width, Height
                        // Adjust coordinates (40, 30) and size (100, 50) as needed
                        gfx.DrawImage(logo, 40, 30, 230, 50);//Relación de 4.6
                    }
                }

                // 2. Define Fonts
                var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
                var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
                var fontBody = new XFont("Arial", 11, XFontStyle.Regular);
                var fontFooter = new XFont("Arial", 10, XFontStyle.Italic);

                // 3. Draw Header Section (IFPES)
                gfx.DrawString($"Fecha: {DateTime.Now.ToString("dd/MMMM/yyyy", culture)}", fontHeader, XBrushes.Black, new XRect(-40, 40, page.Width, 20), XStringFormats.TopRight);

                // 4. Main Title
                gfx.DrawString("Resultado de Control y Confianza", fontTitle, XBrushes.DarkBlue, new XRect(0, 100, page.Width, 40), XStringFormats.Center);

                // 5. Drawing Employee Data Grid
                int startX = 60;
                int currentY = 180;
                int lineSpacing = 25;

                // Helper to draw label/value pairs
                void DrawRow(string label, string value)
                {
                    gfx.DrawString(label, fontHeader, XBrushes.Black, new XPoint(startX, currentY));
                    gfx.DrawString(value ?? "N/A", fontBody, XBrushes.Black, new XPoint(startX + 220, currentY));
                    currentY += lineSpacing;
                }

                DrawRow("Número de empleado:", employee.Id.ToString());
                DrawRow("Nombre:", $"{employee.Name} {employee.LastName}");
                DrawRow("RFC:", employee.Rfc ?? "");
                DrawRow("CURP:", employee.Curp ?? "");
                DrawRow("CUIP:", employee.Cuip ?? "");
            
                currentY += 10; // Extra spacing

                DrawRow("Núm. de oficio resultado SSC:", employee.CeccResults.LastOrDefault()?.OfficialLetter ?? "");
                DrawRow("Fecha del oficio de resultado:", employee.CeccResults.LastOrDefault()?.OfficialLetterDate?.ToString("dd/MMMM/yyyy", culture) ?? "");            

                gfx.DrawString("Resultado:", fontHeader, XBrushes.Black, new XPoint(startX, currentY));
                var resultBrush = employee.CeccResults.LastOrDefault()?.IsApproved == true ? XBrushes.Green : XBrushes.Red;
                gfx.DrawString(employee.CeccResults.LastOrDefault()?.IsApproved == true ? "Aprovado" : "NO aprovado", fontHeader, resultBrush, new XPoint(startX + 220, currentY));
            
                currentY += lineSpacing;                
                gfx.DrawString("Vigencia:", fontHeader, XBrushes.Black, new XPoint(startX, currentY));
                var resultInforceBrush = employee.CeccResults.LastOrDefault()?.IsInforce == true ? XBrushes.Green : XBrushes.Red;
                gfx.DrawString(employee.CeccResults.LastOrDefault()?.IsInforce == true ? "Aún vigente" : "NO vigente", fontHeader, resultInforceBrush, new XPoint(startX + 220, currentY));
                
                currentY += lineSpacing;
                DrawRow("Fecha de vencimiento:", employee.CeccResults.LastOrDefault()?.ExpirationDate?.ToString("dd/MMMM/yyyy", culture) ?? "");

                // 7. Save to stream and return bytes
                using (var stream = new MemoryStream())
                    {
                        document.Save(stream);
                        return stream.ToArray();
                    }
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            return Array.Empty<byte>();
        }
    }

    public async Task<byte[]> GeneratePerformanceEvaluationPdf(PerformanceEvaluationModalDto performanceEvaluation)
    {
        try
        {
            using (var document = new PdfDocument())
            {
                document.Info.Title = "Evaluación de desempeño";
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                XTextFormatter tf = new XTextFormatter(gfx);
                XRect rect = new XRect(0, 0, 100, 50);

                string logoPath = Path.Combine(_env.WebRootPath, "Images", "Logo_Dependencia_inv.png");
                if (File.Exists(logoPath))
                {
                    // Load the image
                    using (XImage logo = XImage.FromFile(logoPath))
                    {
                        // Parameters: Image, X, Y, Width, Height
                        // Adjust coordinates (40, 30) and size (100, 50) as needed
                        gfx.DrawImage(logo, 40, 30, 230, 50);//Relación de 4.6
                    }
                }

                // 2. Define Fonts
                var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
                var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
                var fontBody = new XFont("Arial", 11, XFontStyle.Regular);
                var fontFooter = new XFont("Arial", 10, XFontStyle.Italic);

                // 3. Draw Header Section (IFPES)
                gfx.DrawString($"Fecha: {DateTime.Now.ToString("dd/MMMM/yyyy", culture)}", fontHeader, XBrushes.Black, new XRect(-40, 40, page.Width, 20), XStringFormats.TopRight);

                // 4. Main Title
                gfx.DrawString("Resultado de Control y Confianza", fontTitle, XBrushes.DarkBlue, new XRect(0, 100, page.Width, 40), XStringFormats.Center);

                // 5. Drawing Employee Data Grid
                int startX = 60;
                int currentY = 180;
                int lineSpacing = 25;

                // Helper to draw label/value pairs
                void DrawRow(string label, string value)
                {
                    gfx.DrawString(label, fontHeader, XBrushes.Black, new XPoint(startX, currentY));
                    gfx.DrawString(value ?? "N/A", fontBody, XBrushes.Black, new XPoint(startX + 220, currentY));
                    currentY += lineSpacing;
                }

                DrawRow("Número de empleado:", performanceEvaluation.EmployeeNumber.ToString());
                DrawRow("Nombre:", $"{performanceEvaluation.Name} {performanceEvaluation.LastName}");
                DrawRow("RFC:", performanceEvaluation.Rfc);
                DrawRow("CURP:", performanceEvaluation.Curp);
                DrawRow("CUIP:", performanceEvaluation.Cuip);

                currentY += 10; // Extra spacing

                DrawRow("Calificación final:", performanceEvaluation.FinalTestScore.ToString());

                DrawRow("Nivel de desempeño:", performanceEvaluation.PerformanceLevelId.ToString());
                DrawRow("Fecha de acuse:", performanceEvaluation.AcknowledgementDate?.ToString("dd/MMMM/yyyy", culture));
                DrawRow("Fecha de entrega de la unidad:", performanceEvaluation.UnitDeliveryDate?.ToString("dd/MMMM/yyyy", culture));
                DrawRow("Oficio de entrega:", performanceEvaluation.DeliveryLetter);
                //DrawRow("Observación o incidencia:", performanceEvaluation.Observations);
                //DrawRow("Plan individual de acción:", performanceEvaluation.IndividualActionPlan);

                gfx.DrawString("Observación o incidencia:", fontHeader, XBrushes.Black, new XPoint(startX, currentY));
                tf.DrawString(performanceEvaluation.Observations ?? "", fontBody, XBrushes.Black, new XRect(startX + 220, currentY, 100, 50));

                //currentY += lineSpacing;
                //gfx.DrawString("Plan individual de acción:", fontHeader, XBrushes.Black, new XPoint(startX, currentY));
                //gfx.DrawString(performanceEvaluation.IndividualActionPlan ?? "", fontHeader, XBrushes.Black, new XPoint(startX + 220, currentY));

                //currentY += lineSpacing;
                //DrawRow("Fecha de vencimiento:", performanceEvaluation.CeccResults.LastOrDefault()?.ExpirationDate?.ToString("dd/MMMM/yyyy", culture) ?? "");

                // 7. Save to stream and return bytes
                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    return stream.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            return Array.Empty<byte>();
        }
    }

    //void DrawWrappedRow(string label, string value, int maxWidth = 300)
    //{
    //    // Draw label normally
    //    gfx.DrawString(label, fontHeader, XBrushes.Black, new XPoint(startX, currentY));

    //    value ??= "N/A";

    //    // Measure if the value fits in one line
    //    var size = gfx.MeasureString(value, fontBody);

    //    if (size.Width <= maxWidth)
    //    {
    //        // Fits in one line — draw normally
    //        gfx.DrawString(value, fontBody, XBrushes.Black, new XPoint(startX + 220, currentY));
    //        currentY += lineSpacing;
    //    }
    //    else
    //    {
    //        // Split into words and build lines that fit within maxWidth
    //        var words = value.Split(' ');
    //        var line = new StringBuilder();
    //        bool isFirst = true;

    //        foreach (var word in words)
    //        {
    //            var test = line.Length == 0 ? word : $"{line} {word}";
    //            var testSize = gfx.MeasureString(test, fontBody);

    //            if (testSize.Width > maxWidth && line.Length > 0)
    //            {
    //                // Draw current line
    //                int drawX = isFirst ? startX + 220 : startX + 220; // indent continuation lines if needed
    //                gfx.DrawString(line.ToString(), fontBody, XBrushes.Black, new XPoint(drawX, currentY));
    //                currentY += lineSpacing;
    //                isFirst = false;

    //                // Check if new page is needed
    //                if (currentY > page.Height - 60)
    //                {
    //                    page = document.AddPage();
    //                    gfx = XGraphics.FromPdfPage(page);
    //                    currentY = 60;
    //                }

    //                line.Clear();
    //                line.Append(word);
    //            }
    //            else
    //            {
    //                line.Clear();
    //                line.Append(test);
    //            }
    //        }

    //        // Draw remaining text
    //        if (line.Length > 0)
    //        {
    //            gfx.DrawString(line.ToString(), fontBody, XBrushes.Black, new XPoint(startX + 220, currentY));
    //            currentY += lineSpacing;
    //        }
    //    }
    //}
}
