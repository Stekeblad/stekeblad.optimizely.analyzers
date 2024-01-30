using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Stekeblad.Optimizely.Analyzers.Analyzers
{
	public abstract class MyDiagnosticAnalyzerBase : DiagnosticAnalyzer
	{
		protected static string HelpUrl(string diagnosticsId)
		{
			return $"https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/{diagnosticsId}.md";
		}
	}
}
