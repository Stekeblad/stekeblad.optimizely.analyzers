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
	public class ContentRepositoryAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string GetDefaultDiagnosticId = "SOA1042";
		public const string GetDefaultTitle = "The given content type cannot be instantiated";
		internal const string GetDefaultMessageFormat = "Instances of {0} cannot be created, it's an interface, declared abstract or not decorated with ContentTypeAttribute";
		internal const string GetDefaultCategory = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor GetDefaultRule =
			new DiagnosticDescriptor(GetDefaultDiagnosticId, GetDefaultTitle, GetDefaultMessageFormat, GetDefaultCategory, DiagnosticSeverity.Error,
				true, helpLinkUri: HelpUrl(GetDefaultDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(GetDefaultRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol iContentRepositorySymbol = startContext.Compilation.GetTypeByMetadataName(
					 "EPiServer.IContentRepository");

				INamedTypeSymbol contentTypeAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
					 "EPiServer.DataAnnotations.ContentTypeAttribute");

				if (iContentRepositorySymbol != null && contentTypeAttributeSymbol != null)
				{
					startContext.RegisterOperationAction(
						nodeContext => AnalyzeContentRepoMethodCalls(nodeContext,
							iContentRepositorySymbol,
							contentTypeAttributeSymbol),
						OperationKind.Invocation);
				}
			});
		}

		private static void AnalyzeContentRepoMethodCalls(OperationAnalysisContext context,
			INamedTypeSymbol iContentRepositorySymbol,
			INamedTypeSymbol contentTypeAttributeSymbol)
		{
			var operation = (IInvocationOperation)context.Operation;

			// Ensure the method being called is on a type implementing IContentRepository
			if (!operation.TargetMethod.ContainingType.IsDerivedFrom(iContentRepositorySymbol))
				return;

			// Test for SOA1042 - GetDefault should not be called with content types that can not be instantiated
			if (operation.TargetMethod.Name.Equals("GetDefault"))
			{
				var getDefaultSymbol = operation.TargetMethod;
				var contentTypeSymbol = getDefaultSymbol.TypeArguments.FirstOrDefault();

				// GetDefault need exactly one type argument, nullcheck in case we can get here for non-compiling code
				// or custom extension methods without type argument
				if (contentTypeSymbol is null)
					return;

				if (contentTypeSymbol.IsAbstract
					|| !contentTypeSymbol.HasAttributeDerivedFrom(contentTypeAttributeSymbol))
				{
					// We do not need to check the inheritance of contentTypeSymbol, GetDefault restricts the type parameter to IContentData
					// Further, we do not need to check if contentTypeSymbol is an interface, ContentTypeAttribute is not allowed on interfaces.
					Location location = GetLocation(operation.Syntax);
					var diagnostic = Diagnostic.Create(GetDefaultRule, location, contentTypeSymbol.Name);
					context.ReportDiagnostic(diagnostic);
				}
			}

			// Room for future diagnostics relating to IContentRepostory
		}

		/// <summary>
		/// Tries to get the location of the type argument and falls back to the entire invocation if it fails.
		/// </summary>
		private static Location GetLocation(SyntaxNode syntax)
		{
			if (syntax is InvocationExpressionSyntax invocationSyntax)
			{

				if (invocationSyntax.Expression is MemberAccessExpressionSyntax memberAccessSyntax
					&& memberAccessSyntax.Name is GenericNameSyntax geneicNameSyntax)
				{
					TypeArgumentListSyntax typeArgList = geneicNameSyntax.TypeArgumentList;

					if (typeArgList?.Arguments.Count > 0)
					{
						return typeArgList.Arguments[0].GetLocation();
					}
				}
			}

			return syntax.GetLocation();
		}
	}
}
