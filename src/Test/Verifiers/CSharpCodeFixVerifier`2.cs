﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading;
using System.Threading.Tasks;

namespace Stekeblad.Optimizely.Analyzers.Test
{
	public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
		where TAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
	{
		/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic()"/>
		public static DiagnosticResult Diagnostic()
			=> CSharpCodeFixVerifier<TAnalyzer, TCodeFix, MSTestVerifier>.Diagnostic();

		/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
		public static DiagnosticResult Diagnostic(string diagnosticId)
			=> CSharpCodeFixVerifier<TAnalyzer, TCodeFix, MSTestVerifier>.Diagnostic(diagnosticId);

		/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
		public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
			=> CSharpCodeFixVerifier<TAnalyzer, TCodeFix, MSTestVerifier>.Diagnostic(descriptor);

		/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
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

		/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
		public static async Task VerifyCodeFixAsync(string source, ReferenceAssemblies projectDependencies, string fixedSource)
			=> await VerifyCodeFixAsync(source, projectDependencies, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

		/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult, string)"/>
		public static async Task VerifyCodeFixAsync(string source,
			ReferenceAssemblies projectDependencies,
			DiagnosticResult expected,
			string fixedSource,
			string fixEquivalenceKey = null)
			=> await VerifyCodeFixAsync(source, projectDependencies, new[] { expected }, fixedSource, fixEquivalenceKey);

		/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
		public static async Task VerifyCodeFixAsync(string source,
			ReferenceAssemblies projectDependencies,
			DiagnosticResult[] expected,
			string fixedSource,
			string fixEquivalenceKey = null)
		{
			var test = new Test
			{
				TestCode = source,
				FixedCode = fixedSource,
				ReferenceAssemblies = projectDependencies
			};
			test.ExpectedDiagnostics.AddRange(expected);
			test.CodeActionEquivalenceKey = fixEquivalenceKey;
			await test.RunAsync(CancellationToken.None);
		}
	}
}
