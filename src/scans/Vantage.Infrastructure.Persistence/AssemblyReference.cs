using System.Reflection;

namespace Vantage.Infrastructure.Persistence;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}