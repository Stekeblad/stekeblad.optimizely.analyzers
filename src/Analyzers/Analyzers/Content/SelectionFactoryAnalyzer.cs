using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.Content
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SelectionFactoryAnalyzer : MyDiagnosticAnalyzerBase
	{
		internal const string Category = Constants.Categories.DefiningContent;

		public const string MultipleAttributesDiagnosticId = "SOA1013";
		public const string MultipleAttributesTitle = "Only one of SelectOne, SelectMany, AutoSuggestSelection (or deriving attributes) should be used on a property";
		internal const string MultipleAttributesMessageFormat = "{0} is decorated with multiple selection attributes";

		internal static DiagnosticDescriptor MultipleAttributesRule =
			new DiagnosticDescriptor(MultipleAttributesDiagnosticId, MultipleAttributesTitle, MultipleAttributesMessageFormat, Category,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(MultipleAttributesDiagnosticId));

		public const string UnsupportedPropTypeDiagnosticId = "SOA1014";
		public const string UnsupportedPropTypeTitle = "Type may not work with the attributes SelectOne, SelectMany or AutoSuggestSelection (or deriving attributes)";
		internal const string UnsupportedPropTypeMessageFormat = "{0} is of a type {1} that may not work with {2}";

		internal static DiagnosticDescriptor UnsupportedPropTypeRule =
			new DiagnosticDescriptor(UnsupportedPropTypeDiagnosticId, UnsupportedPropTypeTitle, UnsupportedPropTypeMessageFormat, Category,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(UnsupportedPropTypeDiagnosticId));

		public const string MissingFactoryTypeParamDiagnosticId = "SOA1015";
		public const string MissingFactoryTypeParamTitle = "Attribute is missing the SelectionFactoryType parameter";
		internal const string MissingFactoryTypeParamMessageFormat = "{0} on {1} is missing the SelectionFactoryType parameter";

		internal static DiagnosticDescriptor MissingFactoryTypeParamRule =
			new DiagnosticDescriptor(MissingFactoryTypeParamDiagnosticId, MissingFactoryTypeParamTitle, MissingFactoryTypeParamMessageFormat, Category,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(MissingFactoryTypeParamDiagnosticId));

		public const string InvalidFactoryTypeDiagnosticId = "SOA1016";
		public const string InvalidFactoryTypeTitle = "SelectionFactoryType does not implement ISelectionFactory or is declared abstract";
		internal const string InvalidFactoryTypeMessageFormat = "{0} does not implement ISelectionFactory or is declared abstract";

		internal static DiagnosticDescriptor InvalidFactoryTypeRule =
			new DiagnosticDescriptor(InvalidFactoryTypeDiagnosticId, InvalidFactoryTypeTitle, InvalidFactoryTypeMessageFormat, Category,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(InvalidFactoryTypeDiagnosticId));

		public const string InvalidSelectionQueryTypeDiagnosticId = "SOA1017";
		public const string InvalidSelectionQueryTypeTitle = "SelectionFactoryType does not implement ISelectionQuery or is declared abstract";
		internal const string InvalidSelectionQueryTypeMessageFormat = "{0} does not implement ISelectionQuery or is declared abstract";

		internal static DiagnosticDescriptor InvalidSelectionQueryTypeRule =
			new DiagnosticDescriptor(InvalidSelectionQueryTypeDiagnosticId, InvalidSelectionQueryTypeTitle, InvalidSelectionQueryTypeMessageFormat, Category,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(InvalidSelectionQueryTypeDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(
					MultipleAttributesRule,
					UnsupportedPropTypeRule,
					MissingFactoryTypeParamRule,
					InvalidFactoryTypeRule,
					InvalidSelectionQueryTypeRule);
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol selectOneAttrSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.Shell.ObjectEditing.SelectOneAttribute");

				INamedTypeSymbol selectManyAttrSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.Shell.ObjectEditing.SelectManyAttribute");

				INamedTypeSymbol autoSuggestionAttrSymbol = startContext.Compilation.GetTypeByMetadataName(
						"EPiServer.Shell.ObjectEditing.AutoSuggestSelectionAttribute");

				INamedTypeSymbol iSelectionFactorySymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Shell.ObjectEditing.ISelectionFactory");

				INamedTypeSymbol iSelectionQuerySymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Shell.ObjectEditing.ISelectionQuery");

                INamedTypeSymbol systemInt32Symbol = startContext.Compilation.GetSpecialType(SpecialType.System_Int32);

                INamedTypeSymbol systemStringSymbol = startContext.Compilation.GetSpecialType(SpecialType.System_String);

                if (selectOneAttrSymbol != null)
				{
					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeProperty(nodeContext,
							selectOneAttrSymbol, selectManyAttrSymbol, autoSuggestionAttrSymbol,
							iSelectionFactorySymbol, iSelectionQuerySymbol, systemInt32Symbol, systemStringSymbol),
						SymbolKind.Property);
				}
			});
		}

		public static void AnalyzeProperty(
			SymbolAnalysisContext context,
			INamedTypeSymbol selectOneAttrSymbol,
			INamedTypeSymbol selectManyAttrSymbol,
			INamedTypeSymbol autoSuggestionAttrSymbol,
			INamedTypeSymbol iSelectionFactorySymbol,
			INamedTypeSymbol iSelectionQuerySymbol,
			INamedTypeSymbol systemInt32Symbol,
			INamedTypeSymbol systemStringSymbol)
		{
			var aProp = context.Symbol as IPropertySymbol;

			// Check if and how many of the selection attributes are present on the property
			int attributeCounter = 0;

			if (aProp.TryGetAttributeDerivedFrom(selectOneAttrSymbol, out AttributeData selectOneAttr))
				attributeCounter++;

			if (aProp.TryGetAttributeDerivedFrom(selectManyAttrSymbol, out AttributeData selectManyAttr))
				attributeCounter++;

			if (aProp.TryGetAttributeDerivedFrom(autoSuggestionAttrSymbol, out AttributeData autoSuggestionAttr))
				attributeCounter++;

			if (attributeCounter == 0)
			{
				// Attributes not found on property, nothing more for this analyzer to do
				return;
			}

			// The attributes can not be combined on a property (AllowMultiple=false on AttributeUsageAttribute ensures the same attribute does not appear multiple times)
			// Will however not identify when for example SelectOneAttribute and an attribute deriving from it are both used.
			if (attributeCounter > 1)
			{
				var diagnostic = Diagnostic.Create(MultipleAttributesRule, aProp.Locations[0], aProp.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}

			// Properties with these attributes must be ints or strings (SelectMany may however not work with int when selecting multiple options)
			// Enums are sort of a collection of integers with nice names assigned to them. Optimizely provides an abstract EnumSelectionFactory.
			// Nullable variants of the above is also allowed
			var typeSymbol = aProp.Type as INamedTypeSymbol;
			bool nullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

			// Check if a nullable type and get the inner/real type
			if (nullable && typeSymbol.Arity == 1)
			{
                typeSymbol = typeSymbol.TypeArguments[0] as INamedTypeSymbol;
				nullable = true;
			}

            if (typeSymbol.TypeKind != TypeKind.Enum
                && !SymbolEqualityComparer.Default.Equals(typeSymbol, systemStringSymbol)
				&& !SymbolEqualityComparer.Default.Equals(typeSymbol, systemInt32Symbol))
			{
				var name = typeSymbol.Name + (nullable ? "?" : string.Empty);

                var diagnostic = Diagnostic.Create(UnsupportedPropTypeRule, aProp.Locations[0], aProp.Name, name,
					(selectOneAttr ?? selectManyAttr ?? autoSuggestionAttr).AttributeClass.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}

			// Common checks that are the same for both SelectOne and SelectMany
			// The attribute must have a named parameter called SelectionFactoryType of type Type and it must implement ISelectionFactory
			var oneOrManyAttr = selectOneAttr ?? selectManyAttr;
			if (oneOrManyAttr != null)
			{
				if (!oneOrManyAttr.TryGetArgument("SelectionFactoryType", out TypedConstant selectionFactoryTypeParameter))
				{
					// Optimizely suggest in their documentation that you can inherit from the selection attribute and override
					// the SelectionFactoryType property to have a default value. If the attribute used is a custom one
					// and does not have the parameter this is likely the cause
					// https://docs.developers.optimizely.com/content-cloud/v12.0.0-content-cloud/docs/single-or-multiple-list-options#creating-your-own-attributes
					if (SymbolEqualityComparer.Default.Equals(oneOrManyAttr.AttributeClass, selectOneAttrSymbol)
						|| SymbolEqualityComparer.Default.Equals(oneOrManyAttr.AttributeClass, selectManyAttrSymbol))
					{
						var diagnostic = Diagnostic.Create(MissingFactoryTypeParamRule, oneOrManyAttr.GetLocation(),
							oneOrManyAttr.AttributeClass.Name, aProp.Name);
						context.ReportDiagnostic(diagnostic);
					}
					return;
				}

				// If the parameter is present, make sure the Type implements ISelectionFactory (and is not abstract)
				var paramSymbol = selectionFactoryTypeParameter.Value as INamedTypeSymbol;
				if (!paramSymbol.AllInterfaces.Contains(iSelectionFactorySymbol)
					|| paramSymbol.IsAbstract)
				{
					var diagnostic = Diagnostic.Create(InvalidFactoryTypeRule, oneOrManyAttr.GetLocation(), paramSymbol.Name);
					context.ReportDiagnostic(diagnostic);
					return;
				}
			}

			// The AutoSuggestion attribute is a bit different
			// The attribute constructor must have one parameter of type Type and it must implement ISelectionQuery
			if (autoSuggestionAttr != null)
			{
				// autoSuggestionAttr does not have a public constructor with 0 parameters, in this case
				// the IDE or compiler will complain. The attribute can also be inherited
				// and the custom class may have a 0 argument constructor that provides a value to the base constructor,
				// in that case everything should be fine. Analyzing the custom attribute is out-of-scope for this class.
				// see comment and link on the similar scenario for SelectOne/SelectMany
				if (autoSuggestionAttr.ConstructorArguments.Length == 0)
				{
					// AutoSuggestSelectionAttribute does not have a constructor that takes 0 arguments
					// The IDE or compiler will complain about this so no need to create a diagnostic
					return;
				}

				// If a constructor argument is present, make sure the Type implements ISelectionQuery (and is not abstract)
				var paramSymbol = autoSuggestionAttr.ConstructorArguments[0].Value as INamedTypeSymbol;
				if (!paramSymbol.AllInterfaces.Contains(iSelectionQuerySymbol)
					|| paramSymbol.IsAbstract)
				{
					var diagnostic = Diagnostic.Create(InvalidSelectionQueryTypeRule, autoSuggestionAttr.GetLocation(), paramSymbol.Name);
					context.ReportDiagnostic(diagnostic);
					return;
				}
			}
		}
	}
}
