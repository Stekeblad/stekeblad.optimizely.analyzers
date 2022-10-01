using Microsoft.CodeAnalysis.CodeFixes;

namespace Stekeblad.Optimizely.Analyzers
{
	public abstract class MyCodeFixProviderBase<T> : CodeFixProvider where T : MyDiagnosticAnalyzerBase
	{

	}
}
