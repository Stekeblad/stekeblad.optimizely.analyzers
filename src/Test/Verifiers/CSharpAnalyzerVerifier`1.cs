using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading;
using System.Threading.Tasks;

namespace Stekeblad.Optimizely.Analyzers.Test
{
	public static partial class CSharpAnalyzerVerifier<TAnalyzer>
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
		public static DiagnosticResult Diagnostic()
			=> CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic();

		/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
		public static DiagnosticResult Diagnostic(string diagnosticId)
			=> CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(diagnosticId);

		/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
		public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
			=> CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(descriptor);

		public static async Task VerifyAnalyzerAsync(string source, ReferenceAssemblies projectDependencies, params DiagnosticResult[] expected)
		{
			var test = new Test
			{
				TestCode = source,
				ReferenceAssemblies = projectDependencies
			};

			test.ExpectedDiagnostics.AddRange(expected);
			await test.RunAsync(CancellationToken.None);
		}
	}
}
