using Microsoft.CodeAnalysis.CodeFixes;

namespace Stekeblad.Optimizely.Analyzers
{
#pragma warning disable RS1016 // Code fix providers should provide FixAll support
	public abstract class MyCodeFixProviderBase<T> : CodeFixProvider where T : MyDiagnosticAnalyzerBase
#pragma warning restore RS1016 // Code fix providers should provide FixAll support
	{
	}
}
