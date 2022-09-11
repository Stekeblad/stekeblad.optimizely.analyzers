using Microsoft.CodeAnalysis.Diagnostics;

namespace Stekeblad.Optimizely.Analyzers
{
	public abstract class MyDiagnosticAnalyzerBase : DiagnosticAnalyzer
	{
		protected static string HelpUrl(string diagnosticsId)
			=> $"https://gihub.com/stekeblad/stekeblad.optimizely.analyzers/doc/analyzers/{diagnosticsId}";
	}
}
