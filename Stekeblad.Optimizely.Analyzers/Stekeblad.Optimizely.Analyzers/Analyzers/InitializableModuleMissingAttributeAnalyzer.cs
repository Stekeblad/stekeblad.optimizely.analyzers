using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class InitializableModuleMissingAttributeAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string DiagnosticId = "SOA1004";
		public static readonly LocalizableString Title = "Initializable modules needs an attribute to be discovered";
		internal static readonly LocalizableString MessageFormat = "'{0}' implements IInitializableModule or IConfigurableModule but is not decorated with InitializableModuleAttribute or ModuleDependencyAttribute";
		internal const string Category = Constants.Categories.InitializationModules;

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
				INamedTypeSymbol initModuleAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
					 "EPiServer.Framework.InitializableModuleAttribute");
				INamedTypeSymbol moduleDependsAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Framework.ModuleDependencyAttribute");
				INamedTypeSymbol initModuleInterfaceSymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Framework.IInitializableModule");

				if (initModuleAttributeSymbol != null)
				{
					startContext.RegisterSymbolAction(
						nodeContext => TestForAttribute(nodeContext, initModuleAttributeSymbol, moduleDependsAttributeSymbol, initModuleInterfaceSymbol),
						SymbolKind.NamedType);
				}
			});
		}

		private static void TestForAttribute(SymbolAnalysisContext context,
			INamedTypeSymbol initModuleAttributeSymbol,
			INamedTypeSymbol moduleDependsAttributeSymbol,
			INamedTypeSymbol initModuleInterfaceSymbol)
		{
			var analyzedSymbol = (INamedTypeSymbol)context.Symbol;

			//Check if auto-generated, abstract or not a class.
			if (analyzedSymbol.IsImplicitlyDeclared
				|| analyzedSymbol.TypeKind != TypeKind.Class
				|| analyzedSymbol.IsAbstract)
			{
				return;
			}

			// Test if symbol class implements IInitializableModule
			// No need to test for IConfigurableModule as it implements IInitializableModule
			if (!analyzedSymbol.AllInterfaces.Contains(initModuleInterfaceSymbol))
				return;

			// If either InitializationModuleAttribute or ModuleDependencyAttribute
			// is implemented then there is nothing to report.
			AttributeData _;
			if (analyzedSymbol.TryGetAttributeOrDerivedAttribute(initModuleAttributeSymbol, out _)
				|| analyzedSymbol.TryGetAttributeOrDerivedAttribute(moduleDependsAttributeSymbol, out _))
			{
				return;
			}

			// Type implements IInitializableModule but is not decorated with any of the related attributes, report it
			var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}
}
