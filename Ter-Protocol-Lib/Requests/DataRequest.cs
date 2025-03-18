namespace Ter_Protocol_Lib;

public class DataRequest<T> : IRequest
{
    public T Value { get; set; }
}