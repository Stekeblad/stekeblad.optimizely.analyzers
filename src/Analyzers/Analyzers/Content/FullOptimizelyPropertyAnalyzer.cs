using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.Content
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class FullOptimizelyPropertyAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string FullOptimizelyPropertyDiagnosticId = "SOA1027";
		public const string FullOptimizelyPropertyTitle = "Convert to full Optimizely property";
		internal const string FullOptimizelyPropertyMessageFormat = "Expand the property getter and setter to call the underlying property collection";
		internal const string FullOptimizelyPropertyCategory = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor FullOptimizelyPropertyRule =
			new DiagnosticDescriptor(FullOptimizelyPropertyDiagnosticId, FullOptimizelyPropertyTitle,
				FullOptimizelyPropertyMessageFormat, FullOptimizelyPropertyCategory, DiagnosticSeverity.Hidden,
				true, helpLinkUri: HelpUrl(FullOptimizelyPropertyDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FullOptimizelyPropertyRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol contentDataSymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Core.ContentData");

				if (contentDataSymbol != null)
				{
					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeProperty(nodeContext),
						SymbolKind.Property);
				}
			});
		}

		private void AnalyzeProperty(SymbolAnalysisContext context)
		{
			var analyzedProperty = (IPropertySymbol)context.Symbol;

			if (!analyzedProperty.IsOptiContentProperty(context.Compilation))
				return;

			var getter = analyzedProperty.GetMethod;
			var setter = analyzedProperty.SetMethod;

			// Abort if any of them already has a body
			if (HasAccessorBody(getter) || HasAccessorBody(setter))
			{
				return;
			}

			var diagnostic = Diagnostic.Create(FullOptimizelyPropertyRule, analyzedProperty.Locations[0]);
			context.ReportDiagnostic(diagnostic);
		}

		/// <summary>
		/// Returns true if <paramref name="accessor"/> is a symbol for a getter or setter accessor
		/// and its defined like <c>get;</c> or <c>set;</c>
		/// but not like <c>get => "example"</c> or <c>set { _field = value.Trim(); }</c>
		/// </summary>
		/// <param name="accessor">The method symbol or a property get or set method</param>
		private bool HasAccessorBody(IMethodSymbol accessor)
		{
			if (accessor == null)
				return false;

			// DeclaringSyntaxReferences is a list to support the symbol being defined in multiple places,
			// like in a partial class
			return accessor.DeclaringSyntaxReferences.Any(synRef =>
				!(synRef.GetSyntax() is AccessorDeclarationSyntax ads)
				|| ads.Body != null // get { return x; }
				|| ads.ExpressionBody != null); // get => x;
		}
	}
}
