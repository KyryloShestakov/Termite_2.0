namespace RRLib.Responses;

public class ServerResponseService
{
    public Response GetResponse(bool success, string message, object data = null)
    {
        if (success)
        {
            return new Response
            {
                Status = "success",
                Message = message,
                Data = data
            };
        }
        else
        {
            return new Response
            {
                Status = "error",
                Message = message,
                ErrorCode = "400_BAD_REQUEST"
            };
        }
    }
}