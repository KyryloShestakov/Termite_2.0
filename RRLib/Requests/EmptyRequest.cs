using RRLib;

namespace CommonRequestsLibrary.Requests;

public class EmptyRequest : Request
{
    public string RequestType { get; set; }
    public EmptyRequest(string requestType) : base(requestType)
    {
        RequestType = requestType;
    }
}