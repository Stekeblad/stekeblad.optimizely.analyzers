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
	public class PropertyDefinitionTypePlugInAnalyzer : MyDiagnosticAnalyzerBase
	{
		// SOA1030 -- Missing Attribute
		public const string MissingAttributeDiagnosticId = "SOA1030";
		public const string MissingAttributeTitle = "Missing PropertyDefinitionTypePlugInAttribute on custom property definition";
		internal const string MissingAttributeMessageFormat = "Custom property definition '{0}' has no PropertyDefinitionTypePlugInAttribute and will not be discovered";
		internal const string MissingAttributeCategory = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor MissingAttributeRule =
			new DiagnosticDescriptor(MissingAttributeDiagnosticId, MissingAttributeTitle,
				MissingAttributeMessageFormat, MissingAttributeCategory, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(MissingAttributeDiagnosticId));

		// SOA1031 -- Missing or incompatible base class
		public const string BadBaseClassDiagnosticId = "SOA1031";
		public const string BadBaseClassTitle = "Missing or incompatible base class for property definition";
		internal const string BadBaseClassMessageFormat = "Custom property definition '{0}' does not inherit from a Optimizely property type";
		internal const string BadBaseClassCategory = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor BadBaseClassRule =
			new DiagnosticDescriptor(BadBaseClassDiagnosticId, BadBaseClassTitle,
				BadBaseClassMessageFormat, BadBaseClassCategory, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(BadBaseClassDiagnosticId));

		// SOA1032 -- Missing Guid
		public const string NoGuidDiagnosticId = "SOA1032";
		public const string NoGuidTitle = "Add GUID to PropertyDefinitionTypePlugInAttribute";
		internal const string NoGuidMessageFormat = "Custom property definition '{0}' has not been assigned a GUID, add one to support moving and renaming it";
		internal const string NoGuidCategory = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor NoGuidRule =
			new DiagnosticDescriptor(NoGuidDiagnosticId, NoGuidTitle,
				NoGuidMessageFormat, NoGuidCategory, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(NoGuidDiagnosticId));

		// SOA1033 -- Invalid Guid
		public const string InvalidGuidDiagnosticId = "SOA1033";
		public const string InvalidGuidTitle = "PropertyDefinitionTypePlugInAttribute has an invalid GUID";
		internal const string InvalidGuidMessageFormat = "The GUID on '{0}' is invalid";
		internal const string InvalidGuidCategory = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor InvalidGuidRule =
			new DiagnosticDescriptor(InvalidGuidDiagnosticId, InvalidGuidTitle,
				InvalidGuidMessageFormat, InvalidGuidCategory, DiagnosticSeverity.Error,
				true, helpLinkUri: HelpUrl(InvalidGuidDiagnosticId));

		// SOA1034 -- GUID reused
		public const string GuidReusedDiagnosticId = "SOA1034";
		public const string GuidReusedTitle = "Multiple PropertyDefinitionTypePlugInAttributes must not share GUID";
		internal const string GuidReusedMessageFormat = "{0} does not have a unique GUID. The GUID is used by the following property definitions: {1}.";
		internal const string GuidReusedCategory = Constants.Categories.DefiningContent;

		internal static DiagnosticDescriptor GuidReusedRule =
			new DiagnosticDescriptor(GuidReusedDiagnosticId, GuidReusedTitle,
				GuidReusedMessageFormat, GuidReusedCategory, DiagnosticSeverity.Error,
				true, helpLinkUri: HelpUrl(GuidReusedDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(MissingAttributeRule, BadBaseClassRule, NoGuidRule, InvalidGuidRule, GuidReusedRule);

#pragma warning disable RS1026 // Enable concurrent execution
		public override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			//context.EnableConcurrentExecution(); // This analyzer is not thread safe, adds to a analysis context shared list

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol propertyDefinitionAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
					 "EPiServer.PlugIn.PropertyDefinitionTypePlugInAttribute");
				INamedTypeSymbol propertyDataSymbol = startContext.Compilation.GetTypeByMetadataName(
					"EPiServer.Core.PropertyData");

				if (propertyDefinitionAttributeSymbol != null && propertyDataSymbol != null)
				{
					// The GUID parameter on PropertyDefinitionTypePlugInAttribute was added in EPiServer.CMS.Core version 11.14.0
					// Located in the file Episerver.dll
					bool isGuidParameterSupported = startContext.Compilation.HasReferenceAssembly("EPiServer", new Version(11, 14));

					List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList =
						new List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)>();

					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeSymbol(nodeContext, propertyDefinitionAttributeSymbol,
						propertyDataSymbol, isGuidParameterSupported, guidTypeAttributeList),
						SymbolKind.NamedType);

					startContext.RegisterCompilationEndAction(endContext =>
						Summarize(endContext, guidTypeAttributeList));
				}
			});
		}

		private void AnalyzeSymbol(SymbolAnalysisContext context,
			INamedTypeSymbol propertyDefinitionAttributeSymbol,
			INamedTypeSymbol propertyDataSymbol,
			bool isGuidParameterSupported,
			List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList)
		{
			var analyzedSymbol = context.Symbol as INamedTypeSymbol;

			//Check if auto-generated, abstract or not a class.
			if (analyzedSymbol?.IsImplicitlyDeclared != false
				|| analyzedSymbol.TypeKind != TypeKind.Class
				|| analyzedSymbol.IsAbstract)
			{
				return;
			}

			// Get PropertyDefinitionTypePlugIn attribute
			analyzedSymbol.TryGetAttributeDerivedFrom(propertyDefinitionAttributeSymbol, out var propertyDefinitionAttribute);

			// Check inheritance from PropertyData
			bool derivesFromPropertyData = analyzedSymbol.IsDerivedFrom(propertyDataSymbol);

			if (!derivesFromPropertyData)
			{
				if (propertyDefinitionAttribute is null)
				{
					// No attribute, not a relevant base class, all OK, return
					return;
				}
				else
				{
					// Have attribute but missing relevant base class, diagnose about what to inherit from
					var diagnostic = Diagnostic.Create(BadBaseClassRule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
					context.ReportDiagnostic(diagnostic);
				}
			}

			if (propertyDefinitionAttribute is null)
			{
				// Have supported base class, but not the attribute, diagnose about adding the attribute
				var diagnostic = Diagnostic.Create(MissingAttributeRule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);

				//return, later tests require attribute to be present
				return;
			}

			// From here we know the symbol have the attribute and a base class of the correct type

			if (!isGuidParameterSupported)
				return;

			// Check if the attribute sets the GUID property and if not diagnose about adding a GUID
			if (!propertyDefinitionAttribute.TryGetArgument("GUID", out var propDefGuid))
			{
				var diagnostic = Diagnostic.Create(NoGuidRule, propertyDefinitionAttribute.GetLocation(), analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}

			// Check if the Guid property (a string) is a valid GUID
			if (!(propDefGuid.Value is string guidString)
				|| string.IsNullOrEmpty(guidString)
				|| !Guid.TryParse(guidString, out Guid guidValue))
			{
				var diagnostic = Diagnostic.Create(InvalidGuidRule, propertyDefinitionAttribute.GetLocation(), analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				return;
			}

			// Collect information about all types, attributes and GUIDs to be able to check for duplicates in CompilationEnd phase
			guidTypeAttributeList.Add((guidValue, analyzedSymbol, propertyDefinitionAttribute));
		}

		private void Summarize(CompilationAnalysisContext context, List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList)
		{
			foreach (var typesAttributesWithGuid in guidTypeAttributeList.GroupBy(x => x.Guid))
			{
				// Check if any of the found attributes have the same GUID
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