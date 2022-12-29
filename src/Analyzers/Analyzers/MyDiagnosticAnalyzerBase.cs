using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;
using System.Reflection;

namespace Stekeblad.Optimizely.Analyzers.Analyzers
{
	public abstract class MyDiagnosticAnalyzerBase : DiagnosticAnalyzer
	{
		protected static string HelpUrl(string diagnosticsId)
			=> $"https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/{diagnosticsId}.md";

		protected static string HelpUrl(string diagnosticsId, bool addVersion)
		{
			if (!addVersion)
				return HelpUrl(diagnosticsId);

			var version = GetVersion();

			if (!version.Equals("1.0.0"))
			{
				// Release build, link to documentation at the tag for the installed version
				return $"https://github.com/Stekeblad/stekeblad.optimizely.analyzers/tree/v{version}/doc/Analyzers/{diagnosticsId}.md";
			}
			else
			{
				// Dev build, link to the latest version of the documentation
				return HelpUrl(diagnosticsId);
			}
		}

		private static string _version = null;
		private static string GetVersion()
		{
			if (_version == null)
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
				_version = fileVersionInfo.ProductVersion;
			}
			return _version;
		}
	}
}
