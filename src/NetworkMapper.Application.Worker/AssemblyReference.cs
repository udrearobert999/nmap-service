using System.Reflection;

namespace NetworkMapper.Application.Worker;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}