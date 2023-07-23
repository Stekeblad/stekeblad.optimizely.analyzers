using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.Content
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class UseSetDefaultValuesAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string DiagnosticID = "SOA1019";
		public const string Title = "Initialize property inside SetDefaultValues";
		internal const string MessageFormat = "{0} is assigned a value using property initialization. Content properties should be assigned a default value by overriding the method SetDefaultValues.";

		internal static DiagnosticDescriptor UseSetDefaultValueRule =
			new DiagnosticDescriptor(DiagnosticID, Title, MessageFormat, Constants.Categories.DefiningContent,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(DiagnosticID));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UseSetDefaultValueRule);

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
			var aProp = context.Symbol as IPropertySymbol;
			if (!aProp.IsOptiContentProperty(context.Compilation))
				return;

			var propDeclSyn = aProp.DeclaringSyntaxReferences[0].GetSyntax() as PropertyDeclarationSyntax;

			if (propDeclSyn?.Initializer?.IsMissing == false)
			{
				var diagnostic = Diagnostic.Create(UseSetDefaultValueRule, propDeclSyn.Initializer.GetLocation(), aProp.Name);
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
