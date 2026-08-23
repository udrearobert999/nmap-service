using System.Reflection;

namespace Vantage.Infrastructure.Worker;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}