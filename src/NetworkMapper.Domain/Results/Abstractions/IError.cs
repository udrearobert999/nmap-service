namespace NetworkMapper.Domain.Results.Abstractions;

public interface IError
{
    public string Code { get; init; }

    public string Message { get; init; }

    public IEnumerable<IError> InnerErrors { get; set; }
}