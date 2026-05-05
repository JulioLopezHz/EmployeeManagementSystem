namespace EmployeeManagementSystem.Shared.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Rfc { get; set; } = "";

    //public string Curp { get; set; } = "";

    public string Cuip { get; set; } = "";
    public string PhoneNumber { get; set; } = "";

    public string Email { get; set; } = "";
}
