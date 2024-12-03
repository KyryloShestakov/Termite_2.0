namespace RRLib.Responses;

public class Response
{
    public string Status { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
    public string ErrorCode { get; set; }
    
}