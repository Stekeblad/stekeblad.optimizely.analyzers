using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.Content
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class UseContentTypeAttributeAnalyzer : MyDiagnosticAnalyzerBase
	{
		internal const string Category = Constants.Categories.DefiningContent;

		public const string MissingAttributeDiagnosticId = "SOA1001";
		public const string Title = "Use ContentTypeAttribute";
		internal const string MessageFormat = "Content type '{0}' should be decorated with ContentTypeAttribute or be declared abstract";

		internal static DiagnosticDescriptor MissingAttributeRule =
			new DiagnosticDescriptor(MissingAttributeDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(MissingAttributeDiagnosticId));

		public const string MissingGuidDiagnosticId = "SOA1002";
		public const string GuidTitle = "Add GUID parameter to ContentTypeAttribute";
		internal const string GuidMsgFormat = "'{0}' on '{1}' is missing a GUID, see analyzer help link for important details";

		internal static DiagnosticDescriptor MissingGuidRule =
			new DiagnosticDescriptor(MissingGuidDiagnosticId, GuidTitle, GuidMsgFormat, Category, DiagnosticSeverity.Warning,
				   true, helpLinkUri: HelpUrl(MissingGuidDiagnosticId));

		public const string GuidReusedDiagnosticId = "SOA1009";
		public const string GuidReusedTitle = "Multiple content types must not share GUID";
		public const string GuidReusedMessageFormat = "{0} does not have a unique GUID. The GUID is used by the following content types: {1}.";

		internal static DiagnosticDescriptor GuidReusedRule =
			new DiagnosticDescriptor(GuidReusedDiagnosticId, GuidReusedTitle, GuidReusedMessageFormat, Category,
				DiagnosticSeverity.Error, true, helpLinkUri: HelpUrl(GuidReusedDiagnosticId));

		public const string InvalidGuidDiagnosticId = "SOA1010";
		public const string InvalidGuidTitle = "ContentTypeAttribute has an invalid GUID";
		public const string InvalidGuidMessageFormat = "ContentTypeAttribute on {0} has an invalid GUID";

		internal static DiagnosticDescriptor InvalidGuidRule =
			new DiagnosticDescriptor(InvalidGuidDiagnosticId, InvalidGuidTitle, InvalidGuidMessageFormat, Category,
				DiagnosticSeverity.Error, true, helpLinkUri: HelpUrl(InvalidGuidDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get { return ImmutableArray.Create(MissingAttributeRule, MissingGuidRule, GuidReusedRule, InvalidGuidRule); }
		}

#pragma warning disable RS1026 // Enable concurrent execution
		public override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
			//context.EnableConcurrentExecution(); // This analyzer is not thread safe, adds to a analysis context shared list

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol contentDataSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.Core.ContentData");
				INamedTypeSymbol contentTypeAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.DataAnnotations.ContentTypeAttribute");

				if (contentDataSymbol != null
					&& contentTypeAttributeSymbol != null)
				{
					List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList =
						new List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)>();

					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeNamedTypeSymbol(nodeContext, contentDataSymbol, contentTypeAttributeSymbol, guidTypeAttributeList),
						SymbolKind.NamedType);

					startContext.RegisterCompilationEndAction(x => Summarize(x, guidTypeAttributeList));
				}
			});
		}

		public static void AnalyzeNamedTypeSymbol(
			SymbolAnalysisContext context,
			INamedTypeSymbol contentDataSymbol,
			INamedTypeSymbol contentTypeAttributeSymbol,
			List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList)
		{
			var analyzedSymbol = context.Symbol as INamedTypeSymbol;

			//Check if auto-generated, abstract or not a class. Check its a type of content (media, page or block)
			if (analyzedSymbol?.IsImplicitlyDeclared != false
				|| analyzedSymbol.TypeKind != TypeKind.Class
				|| analyzedSymbol.IsAbstract
				|| !analyzedSymbol.IsDerivedFrom(contentDataSymbol))
			{
				return;
			}

			// Then check it has the ContentTypeAttribute attribute or a custom attribute deriving from that type
			// The attribute is not checked recursively on analyzedSymbol as the attribute can not be inherited
			if (!analyzedSymbol.TryGetAttributeDerivedFrom(contentTypeAttributeSymbol, out AttributeData foundAttributeData))
			{
				var diagnostic = Diagnostic.Create(MissingAttributeRule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}

			if (foundAttributeData?.TryGetArgument("GUID", out TypedConstant guidAttrParam) != true)
			{
				var diagnostic = Diagnostic.Create(MissingGuidRule,
					foundAttributeData.GetLocation(),
					foundAttributeData.AttributeClass.Name,
					analyzedSymbol.Name);

				context.ReportDiagnostic(diagnostic);
				return;
			}

			// guidAttrParam.Value is of type object and can be anything
			// // but it should be a string in the format of a guid
			if (!(guidAttrParam.Value is string guidString)
				|| string.IsNullOrEmpty(guidString)
				|| !Guid.TryParse(guidString, out Guid guidValue))
			{
				var diagnostic = Diagnostic.Create(InvalidGuidRule,
					foundAttributeData.GetLocation(),
					analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}

			// All conditions except if it is unique or not has been checked.
			// Save details about this location and check uniqueness after all types has been analyzed
			guidTypeAttributeList.Add((guidValue, analyzedSymbol, foundAttributeData));
		}

		public void Summarize(CompilationAnalysisContext context, List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList)
		{
			foreach (var typesAttributesWithGuid in guidTypeAttributeList.GroupBy(x => x.Guid))
			{
				if (typesAttributesWithGuid.Count() > 1)
				{
					// The type names are sorted to have a determined order in the test cases
					var typeNames = string.Join(", ", typesAttributesWithGuid.Select(x => x.Type.Name).OrderBy(x => x));
					foreach (var typeAttr in typesAttributesWithGuid)
					{
						var diagnostic = Diagnostic.Create(GuidReusedRule,
						typeAttr.Attribute.GetLocation(),
						typeAttr.Type.Name, typeNames);

						context.ReportDiagnostic(diagnostic);
					}
				}
			}
		}
	}
}
