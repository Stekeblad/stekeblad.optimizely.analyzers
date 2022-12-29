using Microsoft.CodeAnalysis.CodeFixes;
using Stekeblad.Optimizely.Analyzers.Analyzers;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes
{
#pragma warning disable RS1016 // Code fix providers should provide FixAll support
	public abstract class MyCodeFixProviderBase<T> : CodeFixProvider where T : MyDiagnosticAnalyzerBase
#pragma warning restore RS1016 // Code fix providers should provide FixAll support
	{
	}
}
