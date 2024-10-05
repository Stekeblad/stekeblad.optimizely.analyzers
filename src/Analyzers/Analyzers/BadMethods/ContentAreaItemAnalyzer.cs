using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ContentAreaItemAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string ContentAreaItemDiagnosticId = "SOA1029";
		public const string ContentAreaItemTitle = "Don't manually access content from a ContentAreaItem";
		internal const string ContentAreaItemMessageFormat = "Don't use this ContentAreaItem property, if you want to access the referenced content call the extension method LoadContent() on the contentAreaItem or use IContentAreaLoader.LoadContent(contentAreaItem)";
		internal const string ContentAreaItemCategory = Constants.Categories.BadMethods;

		internal static DiagnosticDescriptor ContentAreaItemRule =
			new DiagnosticDescriptor(ContentAreaItemDiagnosticId, ContentAreaItemTitle,
				ContentAreaItemMessageFormat, ContentAreaItemCategory, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(ContentAreaItemDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ContentAreaItemRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				// EPiServer.dll is located in EPiServer.CMS.Core and EPiServer.UI.dll is located in EPiServer.CMS.UI.Core
				if (startContext.Compilation.HasReferenceAssembly("EPiServer.UI", new Version(12, 21))
					&& startContext.Compilation.HasReferenceAssembly("EPiServer", new Version(12, 15)))
				{
					INamedTypeSymbol caItemSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.Core.ContentAreaItem");

					if (caItemSymbol != null)
					{
						startContext.RegisterOperationAction(
							nodeContext => AnalyzePropertyReference(nodeContext, caItemSymbol),
							OperationKind.PropertyReference);
					}
				}
			});
		}

		private void AnalyzePropertyReference(OperationAnalysisContext context, INamedTypeSymbol caItemSymbol)
		{
			var operation = (IPropertyReferenceOperation)context.Operation;
			var referencedProperty = operation.Property;
			var typeWithTheProperty = referencedProperty.ContainingType;

			// If the property is not on a variable of type ContentAreaItem, abort
			if (!SymbolEqualityComparer.Default.Equals(typeWithTheProperty, caItemSymbol))
				return;

			// Only report on get operations, not on assignments to the properties further down
			var valueUsage = operation.GetValueUsageInfo(referencedProperty);
			if (valueUsage != ValueUsageInfo.Read)
				return;

			// ContentLink and ContentGuid have empty/default values if the content item is a inline block
			// InlineBlock is null if the content item is a shared block or another type of content.
			switch (referencedProperty.Name)
			{
				case "ContentGuid":
				case "ContentLink":
				case "InlineBlock":
					var memberExpression = (MemberAccessExpressionSyntax)operation.Syntax;
					var diagnostic = Diagnostic.Create(ContentAreaItemRule, memberExpression.Name.GetLocation());
					context.ReportDiagnostic(diagnostic);
					break;
			}
		}
	}
}
