using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class UseScheduledPluginAttributeAnalyzer : MyDiagnosticAnalyzerBase
	{
		internal const string Category = Constants.Categories.ScheduledJobs;

		public const string MissingAttributeDiagnosticId = "SOA1007";
		public const string MissingAttributeTitle = "Decorate with ScheduledPluginAttribute";
		internal const string MissingAttributeMessageFormat = "The scheduled job '{0}' should be decorated with ScheduledPlugInAttribute or be declared abstract";

		internal static DiagnosticDescriptor MissingAttributeRule =
			new DiagnosticDescriptor(MissingAttributeDiagnosticId, MissingAttributeTitle, MissingAttributeMessageFormat, Category, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(MissingAttributeDiagnosticId));

		public const string MissingGuidDiagnosticId = "SOA1008";
		public const string MissingGuidTitle = "ScheduledPluginAttribute has no GUID";
		internal const string MissingGuidMessageFormat = "The scheduled job '{0}' has no GUID parameter on ScheduledPlugInAttribute";

		internal static DiagnosticDescriptor MissingGuidRule =
			new DiagnosticDescriptor(MissingGuidDiagnosticId, MissingGuidTitle, MissingGuidMessageFormat, Category, DiagnosticSeverity.Info,
				true, helpLinkUri: HelpUrl(MissingGuidDiagnosticId));

		public const string InvalidGuidDiagnosticId = "SOA1011";
		public const string InvalidGuidTitle = "ScheduledPluginAttribute has an invalid GUID";
		public const string InvalidGuidMessageFormat = "ScheduledPluginAttribute on {0} has an invalid GUID";

		internal static DiagnosticDescriptor InvalidGuidRule =
			new DiagnosticDescriptor(InvalidGuidDiagnosticId, InvalidGuidTitle, InvalidGuidMessageFormat, Category,
				DiagnosticSeverity.Error, true, helpLinkUri: HelpUrl(InvalidGuidDiagnosticId));

		public const string GuidReusedDiagnosticId = "SOA1012";
		public const string GuidReusedTitle = "Multiple scheduled jobs must not share GUID";
		public const string GuidReusedMessageFormat = "{0} does not have a unique GUID. The GUID is used by the following Scheduled jobs {1}.";

		internal static DiagnosticDescriptor GuidReusedRule =
			new DiagnosticDescriptor(GuidReusedDiagnosticId, GuidReusedTitle, GuidReusedMessageFormat, Category,
				DiagnosticSeverity.Error, true, helpLinkUri: HelpUrl(GuidReusedDiagnosticId));

		public const string AttributeOnAbstractDiagnosticId = "SOA1039";
		public const string AttributeOnAbstractTitle = "Don't register abstract ScheduledJob";
		public const string AttributeOnAbstractMessageFormat = "Abstract type {0} should not have a ScheduledPluginAttribute";

		internal static DiagnosticDescriptor AttributeOnAbstractRule =
			new DiagnosticDescriptor(AttributeOnAbstractDiagnosticId, AttributeOnAbstractTitle, AttributeOnAbstractMessageFormat, Category,
				DiagnosticSeverity.Error, true, helpLinkUri: HelpUrl(AttributeOnAbstractDiagnosticId));

		public const string MissingNameDiagnosticId = "SOA1041";
		public const string MissingNameTitle = "ScheduledJob has no name";
		public const string MissingNameMessageFormat = "The scheduled job '{0}' has no DisplayName or LanguagePath parameter on ScheduledPlugInAttribute, or they are empty/null";

		internal static DiagnosticDescriptor MissingNameRule =
			new DiagnosticDescriptor(MissingNameDiagnosticId, MissingNameTitle, MissingNameMessageFormat, Category,
				DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(MissingNameDiagnosticId));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(MissingAttributeRule, MissingGuidRule, InvalidGuidRule,
					GuidReusedRule, AttributeOnAbstractRule, MissingNameRule);

#pragma warning disable RS1026 // Enable concurrent execution
		public override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			//context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol attribtuteSymbol = startContext.Compilation.GetTypeByMetadataName(
				"EPiServer.PlugIn.ScheduledPlugInAttribute");

				INamedTypeSymbol interfaceTypeSymbol = startContext.Compilation.GetTypeByMetadataName(
					  "EPiServer.Scheduler.Internal.IScheduledJob");

				if (attribtuteSymbol != null && interfaceTypeSymbol != null)
				{
					List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList =
						new List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)>();

					// The GUID parameter on ScheduledPlugInAttribute was added in EPiServer version 10.3
					// Do not register any action on lower versions
					bool analyzeGuids = startContext.Compilation.HasReferenceAssembly("EPiServer", new Version(10, 3));

					startContext.RegisterSymbolAction(
						nodeContext => AnalyzeJobDefinition(nodeContext, attribtuteSymbol, interfaceTypeSymbol, guidTypeAttributeList, analyzeGuids),
						SymbolKind.NamedType);

					startContext.RegisterCompilationEndAction(endContext =>
						Summarize(endContext, guidTypeAttributeList));
				}
			});
		}

		private static void AnalyzeJobDefinition(SymbolAnalysisContext context,
			INamedTypeSymbol attributeSymbol,
			INamedTypeSymbol interfaceTypeSymbol,
			List<(Guid Guid, INamedTypeSymbol Type, AttributeData Attribute)> guidTypeAttributeList,
			bool analyzeGuids)
		{
			var analyzedSymbol = (INamedTypeSymbol)context.Symbol;

			//Check if auto-generated or not a class.
			if (analyzedSymbol.IsImplicitlyDeclared || analyzedSymbol.TypeKind != TypeKind.Class)
				return;

			// If symbol class does not implement IScheduledJob then abort.
			if (!analyzedSymbol.AllInterfaces.Contains(interfaceTypeSymbol))
				return;

			// Test for ScheduledPlugInAttribute and if not present then report it as missing.
			bool hasAttribute = analyzedSymbol.TryGetAttributeDerivedFrom(attributeSymbol, out AttributeData attributeData);
			if (!hasAttribute)
			{
				// ... unless it's abstract
				if (!analyzedSymbol.IsAbstract)
				{
					var missingAttrDiagnostic = Diagnostic.Create(MissingAttributeRule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
					context.ReportDiagnostic(missingAttrDiagnostic);
				}

				// All checks below depends on an attribute being present
				return;
			}
			// If it is abstract, then report if attribute is present
			else if (hasAttribute && analyzedSymbol.IsAbstract)
			{
				var syntax = analyzedSymbol.DeclaringSyntaxReferences[0].GetSyntax() as ClassDeclarationSyntax;
				var abstractModifier = syntax.GetAbstractModifier();
				var diagnostic = Diagnostic.Create(
					AttributeOnAbstractRule, abstractModifier.GetLocation(), analyzedSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}

			// Test for the name requirement (DisplayName or LanguagePath)
			ValidateScheduleJobName(context, analyzedSymbol, attributeData);

			// older versions did not have a guid attribute and all checks below depends on its existence.
			// If installed version is too old we return here
			if (!analyzeGuids)
				return;

			// Test if the attribute has the GUID parameter and report it if missing
			if (!attributeData.TryGetArgument("GUID", out TypedConstant guidArgument))
			{
				if (!analyzedSymbol.IsAbstract)
				{
					var missingGuidDiagnostic = Diagnostic.Create(MissingGuidRule, attributeData.GetLocation(), analyzedSymbol.Name);
					context.ReportDiagnostic(missingGuidDiagnostic);
				}
				return;
			}

			// guidArgument.Value is of type object and can be anything
			// but it should be a string in the format of a guid
			if (!(guidArgument.Value is string guidString)
				|| string.IsNullOrEmpty(guidString)
				|| !Guid.TryParse(guidString, out Guid guidValue))
			{
				var invalidGuidDiagnostic = Diagnostic.Create(InvalidGuidRule,
					attributeData.GetLocation(),
					analyzedSymbol.Name);
				context.ReportDiagnostic(invalidGuidDiagnostic);
				return;
			}

			// Collect information about all types, attributes and GUIDs to be able to check for duplicates in CompilationEnd phase
			guidTypeAttributeList.Add((guidValue, analyzedSymbol, attributeData));
		}

		/// <summary>
		/// A scheduled job must have a name for it to be registered and for it's meta data to be updated.
		/// The name is decided by either using ScheduledPluginAttribute.LanguagePath to find a string
		/// using the LocalizationService or just using ScheduledPluginAttribute.DisplayName as is.
		/// </summary>
		private static void ValidateScheduleJobName(SymbolAnalysisContext context,
			INamedTypeSymbol analyzedSymbol,
			AttributeData attributeData)
		{
			if (attributeData.TryGetArgument("DisplayName", out TypedConstant displayNameArgument))
			{
				var displayValue = displayNameArgument.Value as string;
				// Optimizely tests if DisplayName is null or empty when validating job name
				if (!string.IsNullOrEmpty(displayValue))
					return;
			}

			if (attributeData.TryGetArgument("LanguagePath", out TypedConstant languagePathArgument))
			{
				var languagePath = displayNameArgument.Value as string;
				//Optimizely only checks if LanguagePath is null when validating job name
				if (languagePath != null)
					return;
			}

			var diagnostic = Diagnostic.Create(MissingNameRule, attributeData.GetLocation(), analyzedSymbol.Name);
			context.ReportDiagnostic(diagnostic);
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