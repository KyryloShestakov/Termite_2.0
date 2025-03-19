namespace Ter_Protocol_Lib.Requests;

public class DataRequest<T> : IRequest
{
    public T Value { get; set; }
}