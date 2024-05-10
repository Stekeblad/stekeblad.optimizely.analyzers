using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class InitializableModuleMissingAttributeAnalyzer : MyDiagnosticAnalyzerBase
	{
		public const string DiagnosticId = "SOA1004";
		public const string Title = "Initializable modules needs an attribute to be discovered";
		internal const string MessageFormat = "'{0}' implements IInitializableModule or IConfigurableModule but is not decorated with InitializableModuleAttribute or ModuleDependencyAttribute";
		internal const string Category = Constants.Categories.InitializationModules;

		internal static DiagnosticDescriptor Rule =
			new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(DiagnosticId));

        public const string BadTypeDiagnosticId = "SOA1025";
        public const string BadTypeTitle = "Dependencies to initializable modules must be initializable modules";
        internal const string BadTypeMessageFormat = "'{0}' declares dependency on '{1}' but that type is not an initializable module";
        internal const string BadTypeCategory = Constants.Categories.InitializationModules;

        internal static DiagnosticDescriptor BadTypeRule =
            new DiagnosticDescriptor(BadTypeDiagnosticId, BadTypeTitle, BadTypeMessageFormat, BadTypeCategory, DiagnosticSeverity.Error,
                true, helpLinkUri: HelpUrl(BadTypeDiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics{ get { return ImmutableArray.Create(Rule, BadTypeRule); } }

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

			// If analyzed symbol is decorated with ModuleDependencyAttribute
			// then check so all parameters implement IInitializableModule
			if (analyzedSymbol.TryGetAttributesDerivedFrom(moduleDependsAttributeSymbol,
				out List<AttributeData> dependencyAttributeData))
			{
				foreach (var attributeData in dependencyAttributeData)
				{
					var attributeCtorParams = attributeData.ConstructorArguments;

					// The attribute has two constructors, both take one parameter.
					// One of them accepts `Type`, the other `params Type`
					if (attributeCtorParams.Length == 0)
						continue;

                    TypedConstant dependenciesParameter = attributeCtorParams[0];
					if (dependenciesParameter.Kind == TypedConstantKind.Type)
					{
                        INamedTypeSymbol dependencyType = dependenciesParameter.Value as INamedTypeSymbol;
						if (dependencyType.AllInterfaces.Contains(initModuleInterfaceSymbol))
							continue;

                        // Report, ModuleDependency declares that this Initializable module depends on a type that is not an Initializable module
                        var diagnostic = Diagnostic.Create(BadTypeRule, attributeData.GetLocation(), analyzedSymbol.Name, dependencyType.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                    else if (dependenciesParameter.Kind == TypedConstantKind.Array)
					{
                        foreach (TypedConstant dependency in dependenciesParameter.Values)
						{
                            INamedTypeSymbol dependencyType = dependency.Value as INamedTypeSymbol;
                            if (dependencyType.AllInterfaces.Contains(initModuleInterfaceSymbol))
                                continue;

                            // Report, ModuleDependency declares this Initializable module depends on a type that is not an Initializable module
                            var diagnostic = Diagnostic.Create(BadTypeRule, attributeData.GetLocation(), analyzedSymbol.Name, dependencyType.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
				}
			}
			// If no ModuleDependencyAttribute nor InitializationModuleAttribute is present then report.
			else if (!analyzedSymbol.TryGetAttributeDerivedFrom(initModuleAttributeSymbol, out _))
			{
				var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
