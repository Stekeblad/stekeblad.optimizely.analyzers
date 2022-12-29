using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

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
		public const string GuidReusedMessageFormat = "Content type {0} has the same GUID as {1}";

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

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol contentDataSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.Core.ContentData");
				INamedTypeSymbol contentTypeAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.DataAnnotations.ContentTypeAttribute");

				if (contentDataSymbol != null
					&& contentTypeAttributeSymbol != null)
				{
					ConcurrentDictionary<Guid, (INamedTypeSymbol Type, AttributeData Attribute)> contentTypeGuids =
						new ConcurrentDictionary<Guid, (INamedTypeSymbol Type, AttributeData Attribute)>();

					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeNamedTypeSymbol(nodeContext, contentDataSymbol, contentTypeAttributeSymbol, contentTypeGuids),
						SymbolKind.NamedType);
				}
			});
		}

		public static void AnalyzeNamedTypeSymbol(
			SymbolAnalysisContext context,
			INamedTypeSymbol contentDataSymbol,
			INamedTypeSymbol contentTypeAttributeSymbol,
			ConcurrentDictionary<Guid, (INamedTypeSymbol Type, AttributeData Attribute)> contentTypeGuids)
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

			// Two content types must not have the same GUID, make sure it is unique
			var (existingType, existingAttribute) = contentTypeGuids.GetOrAdd(guidValue, (analyzedSymbol, foundAttributeData));
			if (!SymbolEqualityComparer.Default.Equals(existingType, analyzedSymbol))
			{
				var diagnostic1 = Diagnostic.Create(GuidReusedRule,
					foundAttributeData.GetLocation(),
					analyzedSymbol.Name, existingType.Name);

				var diagnostic2 = Diagnostic.Create(GuidReusedRule,
					existingAttribute.GetLocation(),
					existingType.Name, analyzedSymbol.Name);

				context.ReportDiagnostic(diagnostic1);
				context.ReportDiagnostic(diagnostic2);
			}
		}
	}
}
