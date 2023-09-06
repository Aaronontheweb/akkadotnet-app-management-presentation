namespace Akka.Pattern.Common;

public interface IDomainQuery
{
    
}

public enum QueryResponseCode
{
    Found,
    NotFound,
    BadQuery
}

public interface IQueryResponse
{
    QueryResponseCode ResponseCode { get; }
    
    string? Message { get; }
}