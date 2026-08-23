using System.Reflection;

namespace NetworkMapper.Infrastructure.Worker;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}