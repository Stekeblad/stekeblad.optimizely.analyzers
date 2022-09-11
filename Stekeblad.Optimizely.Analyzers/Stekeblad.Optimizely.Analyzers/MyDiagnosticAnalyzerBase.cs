using Microsoft.CodeAnalysis.Diagnostics;

namespace Stekeblad.Optimizely.Analyzers
{
	public abstract class MyDiagnosticAnalyzerBase : DiagnosticAnalyzer
	{
		protected static string HelpUrl(string diagnosticsId)
			=> $"https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/{diagnosticsId}";
	}
}
