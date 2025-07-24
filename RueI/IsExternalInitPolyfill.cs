// Polyfill for IsExternalInit, so users can use init-only properties

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // File name should match first type name
internal static class IsExternalInit
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1600 // Elements should be documented
{
}