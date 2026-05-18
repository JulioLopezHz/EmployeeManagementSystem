namespace EmployeeManagementSystem.Shared.DTOs;

public class PerformanceEvaluationModalDto
{
    public int EmployeeNumber { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;
    public string? LastPayrollPosition { get; set; }

    public string Rfc { get; set; } = "";

    public string Curp { get; set; } = "";

    public string Cuip { get; set; } = "";

    public string PhoneNumber { get; set; } = "";
    public decimal FinalTestScore { get; set; }

    public int? PerformanceLevelId { get; set; }

    public DateOnly? AcknowledgementDate { get; set; }

    public DateOnly? UnitDeliveryDate { get; set; }

    public string? DeliveryLetter { get; set; }

    public string? Observations { get; set; }

    public string? IndividualActionPlan { get; set; }
    
}
