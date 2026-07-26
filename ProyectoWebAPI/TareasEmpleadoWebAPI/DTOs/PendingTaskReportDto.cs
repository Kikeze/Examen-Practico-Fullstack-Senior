namespace TareasEmpleadoWebAPI.DTOs;

public class PendingTaskReportDto
{
    public int UserId { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public int TotalPendientes { get; set; }

    public int TotalVencidas { get; set; }
}
