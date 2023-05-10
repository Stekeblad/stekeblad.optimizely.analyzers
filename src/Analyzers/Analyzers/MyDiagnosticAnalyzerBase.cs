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
			var version = GetVersion();
			return $"https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/{version}/doc/Analyzers/{diagnosticsId}.md";
		}

		private static string _version = null;
		private static string GetVersion()
		{
			if (_version == null)
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
				_version = fileVersionInfo.ProductVersion.Equals("1.0.0")
					? "master" // Dev build, link to the latest version of the documentation
					: $"v{fileVersionInfo.ProductVersion}"; // Release build, link to documentation at the tag for the installed version
			}
			return _version;
		}
	}
}
