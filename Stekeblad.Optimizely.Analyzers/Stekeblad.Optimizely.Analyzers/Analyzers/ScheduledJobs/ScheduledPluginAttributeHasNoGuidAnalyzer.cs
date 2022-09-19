using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ScheduledPluginAttributeHasNoGuidAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string DiagnosticId = "SOA1008";
		public static readonly LocalizableString Title = "ScheduledPluginAttribute has no GUID";
		internal static readonly LocalizableString MessageFormat = "The scheduled job '{0}' has no GUID parameter on ScheduledPlugInAttribute";
		internal const string Category = Constants.Categories.ScheduledJobs;

		internal static DiagnosticDescriptor Rule =
			new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info,
				true, helpLinkUri: HelpUrl(DiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol attribtuteSymbol = startContext.Compilation.GetTypeByMetadataName(
				"EPiServer.PlugIn.ScheduledPlugInAttribute");

				if (attribtuteSymbol != null)
				{
					// The GUID parameter on ScheduledPlugInAttribute was added in EPiServer version 10.3
					// Do not register any action on lower versions
					var assemblyIdentity = startContext.Compilation.ReferencedAssemblyNames.FirstOrDefault(
						assem => assem.Name.Equals("EPiServer"));

					if (assemblyIdentity != null && assemblyIdentity.Version >= new System.Version(10, 3))
					{
						startContext.RegisterSymbolAction(
							nodeContext => AnalyzeJobDefinition(nodeContext, attribtuteSymbol),
							SymbolKind.NamedType);
					}
				}
			});
		}

		private static void AnalyzeJobDefinition(SymbolAnalysisContext context, INamedTypeSymbol attribtuteSymbol)
		{
			var analyzedSymbol = (INamedTypeSymbol)context.Symbol;

			//Check if auto-generated, abstract or not a class.
			if (analyzedSymbol.IsImplicitlyDeclared
				|| analyzedSymbol.TypeKind != TypeKind.Class
				|| analyzedSymbol.IsAbstract)
			{
				return;
			}

			// Test for ScheduledPlugInAttribute and if not present then abort.
			if (!analyzedSymbol.TryGetAttributeOrDerivedAttribute(attribtuteSymbol, out AttributeData attributeData))
			{
				return;
			}

			// If attrribute has GUID parameter, then there is nothing to report
			if (attributeData.NamedArguments.Any(kvp => kvp.Key.Equals("GUID")))
			{
				return;
			}

			// GUID missing, report it.
			var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}
}