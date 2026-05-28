namespace EduTrackAnalytics.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public int StatusCode { get; set; } = 500;

    public string Title { get; set; } = "Something went wrong";

    public string Message { get; set; } = "Something went wrong. Please go back to the dashboard or try again.";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
