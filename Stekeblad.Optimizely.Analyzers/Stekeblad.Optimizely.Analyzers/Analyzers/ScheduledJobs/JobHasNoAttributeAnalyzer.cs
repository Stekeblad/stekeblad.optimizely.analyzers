using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class JobHasNoAttributeAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string DiagnosticId = "SOA1007";
		public static readonly LocalizableString Title = "Decorate with ScheduledPluginAttribute";
		internal static readonly LocalizableString MessageFormat = "The scheduled job '{0}' is not decorated with ScheduledPlugInAttribute";
		internal const string Category = Constants.Categories.ScheduledJobs;

		internal static DiagnosticDescriptor Rule =
			new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
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
				INamedTypeSymbol interfaceTypeSymbol = startContext.Compilation.GetTypeByMetadataName(
					  "EPiServer.Scheduler.Internal.IScheduledJob");

				if (attribtuteSymbol != null && interfaceTypeSymbol != null)
				{
					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeJobDefinition(nodeContext, attribtuteSymbol, interfaceTypeSymbol),
						SymbolKind.NamedType);
				}
			});
		}

		private static void AnalyzeJobDefinition(SymbolAnalysisContext context, INamedTypeSymbol attribtuteSymbol, INamedTypeSymbol interfaceTypeSymbol)
		{
			var analyzedSymbol = (INamedTypeSymbol)context.Symbol;

			//Check if auto-generated, abstract or not a class.
			if (analyzedSymbol.IsImplicitlyDeclared
				|| analyzedSymbol.TypeKind != TypeKind.Class
				|| analyzedSymbol.IsAbstract)
			{
				return;
			}

			// If symbol class does not implement IScheduledJob then abort.
			if (!analyzedSymbol.AllInterfaces.Contains(interfaceTypeSymbol))
				return;

			// Test for ScheduledPlugInAttribute and if present then we have nothing to report.
			if (analyzedSymbol.TryGetAttributeOrDerivedAttribute(attribtuteSymbol, out _))
			{
				return;
			}

			// Symbol implement/inherit required interface/base class but is not decorated with ScheduledPlugInAttribute. Report it
			var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}
}
