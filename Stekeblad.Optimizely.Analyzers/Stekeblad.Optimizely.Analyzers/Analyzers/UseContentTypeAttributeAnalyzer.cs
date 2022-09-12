using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class UseContentTypeAttributeAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string DiagnosticId = "SOA1001";
		public static readonly LocalizableString Title = "Use ContentTypeAttribute";
		internal static readonly LocalizableString MessageFormat = "Content type '{0}' should be decorated with ContentTypeAttribute";
		internal const string Category = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor Rule =
			new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(DiagnosticId));

		public const string GuidDiagnosticId = "SOA1002";
		public static readonly LocalizableString GuidTitle = "Add GUID parameter to ContentTypeAttribute";
		internal static readonly LocalizableString GuidMsgFormat = "'{0}' on '{1}' is missing a GUID, see analyzer help link for important details";

		public static DiagnosticDescriptor SecondRule =
			new DiagnosticDescriptor(GuidDiagnosticId, GuidTitle, GuidMsgFormat, Category, DiagnosticSeverity.Warning,
				   true, helpLinkUri: HelpUrl(GuidDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule, SecondRule); } }

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
						startContext.RegisterSymbolAction(
							nodeContext => AnalyzeNamedTypeSymbol(nodeContext, contentDataSymbol, contentTypeAttributeSymbol),
							SymbolKind.NamedType);
					}
			});
		}

		public static void AnalyzeNamedTypeSymbol(
			SymbolAnalysisContext context,
			INamedTypeSymbol contentDataSymbol,
			INamedTypeSymbol contentTypeAttributeSymbol)
		{
			var analyzedSymbol = context.Symbol as INamedTypeSymbol;

			//Check if auto-generated, abstract or not a class.
			if (analyzedSymbol?.IsImplicitlyDeclared != false
				|| analyzedSymbol.TypeKind != TypeKind.Class
				|| analyzedSymbol.IsAbstract)
			{
				return;
			}

			// Check its a type of content (media, page or block)
			// Then check it has the ContentTypeAttribute attribute or a custom attribute deriving from that type
			// The attribute is not checked recursively on analyzedSymbol as the attribute can not be inherited
			AttributeData foundAttributeData = null;
			if (analyzedSymbol.IsDerivedFrom(contentDataSymbol)
				&& !analyzedSymbol.TryGetAttributeOrDerivedAttribute(contentTypeAttributeSymbol, out foundAttributeData))
			{
				if (analyzedSymbol.Locations.Length == 0)
					throw new Exception("Locations is empty!");

				var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}

			// TODO: Here should the location of the attribute be returned, not the class declaration,
			// // however foundAttributeData.AttributeClass.Locations contain only 0, 0 locations
			// It appears difficult to find exactly this attribute instance in a code fix
			if (foundAttributeData?.NamedArguments.Any(x => x.Key.Equals("GUID", StringComparison.Ordinal)) == false)
			{
				var diagnostic = Diagnostic.Create(SecondRule, analyzedSymbol.Locations[0], foundAttributeData.AttributeClass.Name, analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}
		}
	}
}
