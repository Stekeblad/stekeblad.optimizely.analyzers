using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.Content
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ContentTypeConstructorAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string ConstructorParametersDiagnosticID = "SOA1035";
		public const string ConstructorParametersTitle = "Content type constructor has parameters";
		internal const string ConstructorParametersMessageFormat = "Constructors with parameters is not supported by default for content types and requires additional configuration";

		internal static DiagnosticDescriptor ConstructorParametersRule =
			new DiagnosticDescriptor(ConstructorParametersDiagnosticID, ConstructorParametersTitle,
				ConstructorParametersMessageFormat, Constants.Categories.DefiningContent,
				DiagnosticSeverity.Error, true, helpLinkUri: HelpUrl(ConstructorParametersDiagnosticID));

		public const string ConstructorInitializationDiagnosticID = "SOA1036";
		public const string ConstructorInitializationTitle = "Content property initialized inside constructor";
		internal const string ConstructorInitializationMessageFormat = "Attempting to set default values for content properties set inside constructors throws exception at runtime, set inside SetDefaultValues instead";

		internal static DiagnosticDescriptor ConstructorInitializationRule =
			new DiagnosticDescriptor(ConstructorInitializationDiagnosticID, ConstructorInitializationTitle,
				ConstructorInitializationMessageFormat, Constants.Categories.DefiningContent,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(ConstructorInitializationDiagnosticID));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(ConstructorParametersRule, ConstructorInitializationRule);

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
					startContext.RegisterOperationAction(
						operationContext => AnalyzeConstructorOperation(operationContext, contentDataSymbol),
						OperationKind.ConstructorBody);
				}
			});
		}

		private void AnalyzeConstructorOperation(OperationAnalysisContext context, INamedTypeSymbol contentDataSymbol)
		{
			if (!context.ContainingSymbol.ContainingType.IsDerivedFrom(contentDataSymbol))
				return;

			var conBody = context.Operation as IConstructorBodyOperation;

			// Test for constructor parameters
			var parameterListSyntax = (conBody.Syntax as ConstructorDeclarationSyntax).ParameterList;
			if (parameterListSyntax.Parameters.Count > 0)
			{
				var diagnostic = Diagnostic.Create(ConstructorParametersRule, parameterListSyntax.GetLocation());
				context.ReportDiagnostic(diagnostic);
			}

			// Test if constructor assigns a value to any Optimizely Content Properties
			foreach (var assignmentInCtor in conBody.BlockBody.Operations
				.SelectMany(o => o.Children)
				.OfType<ISimpleAssignmentOperation>())
			{
				if (assignmentInCtor.Target is IPropertyReferenceOperation propRef
					&& propRef.Property.IsOptiContentProperty(context.Compilation))
				{
					var diagnostic = Diagnostic.Create(ConstructorInitializationRule, assignmentInCtor.Syntax.GetLocation());
					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}
}
