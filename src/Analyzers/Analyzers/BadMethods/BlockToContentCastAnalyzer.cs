using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class BlockToContentCastAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string BlockToContentCastDiagnosticId = "SOA1028";
		public const string BlockToContentCastTitle = "Block type is cast to IContent";
		internal const string BlockToContentCastMessageFormat = "It's not safe to assume a block can be cast to IContent";
		internal const string BlockToContentCastCategory = Constants.Categories.BadMethods;

		internal static DiagnosticDescriptor BlockToContentCastRule =
			new DiagnosticDescriptor(BlockToContentCastDiagnosticId, BlockToContentCastTitle,
				BlockToContentCastMessageFormat, BlockToContentCastCategory, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(BlockToContentCastDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(BlockToContentCastRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol iContentSymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Core.IContent");

				INamedTypeSymbol blockDataSymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Core.BlockData");

				if (iContentSymbol != null && blockDataSymbol != null)
				{
					startContext.RegisterOperationAction(
						nodeContext => AnalyzeConversion(nodeContext, iContentSymbol, blockDataSymbol),
						OperationKind.Conversion);
				}
			});
		}

		private void AnalyzeConversion(OperationAnalysisContext context, INamedTypeSymbol iContentSymbol, INamedTypeSymbol blockDataSymbol)
		{
			var operation = (IConversionOperation)context.Operation;
			var resultingType = operation.Type;
			var sourceType = operation.Operand.Type;

			// Create a diagnostic if a block (type derived from BlockData) gets cast to IContent
			// In some cases will blocks "implement IContent at runtime" but this is not always the case,
			// for example property blocks or instances of inline blocks can not be cast to IContent

			// Let's not create any diagnostics for "try casts" (that means "x as IContent" and "x is IContent content" casts)
			// and only complain on forceful casts "(IContent)x" that can throw exceptions at runtime
			if (!operation.IsTryCast
				&& SymbolEqualityComparer.Default.Equals(resultingType, iContentSymbol)
				&& sourceType.IsDerivedFrom(blockDataSymbol))
			{
				var diagnostic = Diagnostic.Create(BlockToContentCastRule, operation.Syntax.GetLocation());
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
