using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

		public const string AttributeOnAbstractDiagnosticId = "SOA1040";
		public const string AttributeOnAbstractTitle = "Don't register abstract Initialization module";
		internal const string AttributeOnAbstractMessageFormat = "Abstract type '{0}' can not have a InitializableModule or ModuleDependency attribute";
		internal const string AttributeOnAbstractCategory = Constants.Categories.InitializationModules;

		internal static DiagnosticDescriptor AttributeOnAbstractRule =
			new DiagnosticDescriptor(AttributeOnAbstractDiagnosticId, AttributeOnAbstractTitle,
				AttributeOnAbstractMessageFormat, AttributeOnAbstractCategory, DiagnosticSeverity.Error,
				true, helpLinkUri: HelpUrl(AttributeOnAbstractDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Rule, BadTypeRule, AttributeOnAbstractRule);

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

			//Check if auto-generated or not a class.
			if (analyzedSymbol.IsImplicitlyDeclared || analyzedSymbol.TypeKind != TypeKind.Class)
				return;

			// Test if symbol class implements IInitializableModule
			// No need to test for IConfigurableModule as it implements IInitializableModule
			if (!analyzedSymbol.AllInterfaces.Contains(initModuleInterfaceSymbol))
				return;

			// Search for the relevant attributes
			// For InitializableModuleAttribute it's enough to know if it is present or not
			// While ModuleDependencyAttribute can be further analyzed
			bool hasAttributes = analyzedSymbol.TryGetAttributeDerivedFrom(initModuleAttributeSymbol,
				out var _);
			hasAttributes |= analyzedSymbol.TryGetAttributesDerivedFrom(moduleDependsAttributeSymbol,
				out List<AttributeData> moduleDependencyAttributes);

			if (!hasAttributes)
			{
				if (!analyzedSymbol.IsAbstract)
				{
					// Report missing attribute
					var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
					context.ReportDiagnostic(diagnostic);
				}

				// No attributes, nothing more to test
				return;
			}
			else if (hasAttributes && analyzedSymbol.IsAbstract)
			{
				// Error: class is abstract but decorated with any of the attributes
				var syntax = analyzedSymbol.DeclaringSyntaxReferences[0].GetSyntax() as ClassDeclarationSyntax;
				var abstractModifier = syntax.GetAbstractModifier();
				var diagnostic = Diagnostic.Create(AttributeOnAbstractRule, abstractModifier.GetLocation(), analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}

			if (moduleDependencyAttributes != null)
			{
				// Local test-and-report method, called in the loop below from multiple branches
				void AnalyzeDependency(TypedConstant dependency, AttributeData attributeData, int index)
				{
					INamedTypeSymbol dependencyType = dependency.Value as INamedTypeSymbol;
					if (!dependencyType.AllInterfaces.Contains(initModuleInterfaceSymbol))
					{
						// Report, a ModuleDependencyAttribute declares this Initializable module depending on a type that is not an Initializable module
						var syntax = attributeData.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax;
						var argSyntax = syntax.ArgumentList.Arguments[index];
						var diagnostic = Diagnostic.Create(
							BadTypeRule, argSyntax.GetLocation(), analyzedSymbol.Name, dependencyType.Name);

						context.ReportDiagnostic(diagnostic);
					}
				}

				// Analyze all ModuleDependencyAttribute found on the type
				foreach (var attributeData in moduleDependencyAttributes)
				{
					var attributeCtorParams = attributeData.ConstructorArguments;

					// The attribute has two constructors, both take one parameter.
					// One of them accepts `Type`, the other `params Type`
					// There is also the option to not provide an argument
					if (attributeCtorParams.Length == 0)
						continue;

					TypedConstant dependenciesParameter = attributeCtorParams[0];
					if (dependenciesParameter.Kind == TypedConstantKind.Type)
					{
						AnalyzeDependency(dependenciesParameter, attributeData, 0);
					}
					else if (dependenciesParameter.Kind == TypedConstantKind.Array)
					{
						for (int i = 0; i < dependenciesParameter.Values.Length; i++)
						{
							AnalyzeDependency(dependenciesParameter.Values[i], attributeData, i);
						}
					}
				}
			}
		}
	}
}
