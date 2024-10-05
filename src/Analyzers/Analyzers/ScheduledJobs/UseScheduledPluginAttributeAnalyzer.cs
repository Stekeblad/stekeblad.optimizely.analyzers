using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Concurrent;
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

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get { return ImmutableArray.Create(MissingAttributeRule, MissingGuidRule, InvalidGuidRule, GuidReusedRule); }
		}

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

			// Test for ScheduledPlugInAttribute and if not present then report it as missing.
			if (!analyzedSymbol.TryGetAttributeDerivedFrom(attributeSymbol, out AttributeData attributeData))
			{
				var missingAttrDiagnostic = Diagnostic.Create(MissingAttributeRule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
				context.ReportDiagnostic(missingAttrDiagnostic);
				return;
			}

			// older versions did not have a guid attribute and all checks below depends on its existence.
			// If installed version is too old we return here
			if (!analyzeGuids)
				return;

			// Test if the attribute has the GUID parameter and report it if missing
			if (!attributeData.TryGetArgument("GUID", out TypedConstant guidArgument))
			{
				var missingGuidDiagnostic = Diagnostic.Create(MissingGuidRule, attributeData.GetLocation(), analyzedSymbol.Name);
				context.ReportDiagnostic(missingGuidDiagnostic);
				return;
			}

			// guidArgument.Value is of type object and can be anything
			// // but it should be a string in the format of a guid
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