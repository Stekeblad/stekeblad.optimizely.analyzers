using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.Content
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ContentPropertyMustBeVirtualAnalyzer : MyDiagnosticAnalyzerBase
    {
        public const string DiagnosticId = "SOA1003";
        public const string Title = "Public non-static properties must be declared virtual";
        internal const string MessageFormat = "'{0}' must be declared as virtual or Optimizely will throw an exception during startup";
        internal const string Category = Constants.Categories.DefiningContent;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error,
                true, helpLinkUri: HelpUrl(DiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol contentDataSymbol = startContext.Compilation.GetTypeByMetadataName(
                    "EPiServer.Core.ContentData");

					INamedTypeSymbol ignoreAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
                        "EPiServer.DataAnnotations.IgnoreAttribute");

				if (contentDataSymbol != null && ignoreAttributeSymbol != null)
				{
					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeNamedTypeSymbol(nodeContext, contentDataSymbol, ignoreAttributeSymbol),
						SymbolKind.NamedType);
				}
			});
        }

        private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context,
            INamedTypeSymbol contentDataSymbol,
			INamedTypeSymbol ignoreAttributeSymbol)
        {
            var analyzedSymbol = (INamedTypeSymbol)context.Symbol;

            if (!analyzedSymbol.IsDerivedFrom(contentDataSymbol))
                return;

            foreach (var prop in analyzedSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.DeclaredAccessibility == Accessibility.Public
                    && !prop.IsImplicitlyDeclared // skip autogen props
                    && !prop.IsStatic
                    && !prop.IsVirtual
                    && !prop.IsOverride // assume overridden prop is valid or reported when base type is analyzed
                    && prop.SetMethod != null // valid example from Alloy: public string ContentAreaCssClass => "teaserblock";
                    // properties decorated with IgnoreAttribute does not show up in the CMS and can be ignored by this analyzer
                    && !prop.HasAttributeDerivedFrom(ignoreAttributeSymbol))
                {
                    var diagnostic = Diagnostic.Create(Rule, prop.Locations[0], prop.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
