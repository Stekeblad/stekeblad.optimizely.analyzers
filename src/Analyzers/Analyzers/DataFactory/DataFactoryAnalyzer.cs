using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.DataFactory
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class DataFactoryAnalyzer : MyDiagnosticAnalyzerBase
	{
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(startContext =>
			{
				INamedTypeSymbol dataFactorySymbol = startContext.Compilation.GetTypeByMetadataName(
					 "EPiServer.DataFactory");

				// DataFactory moved to another assembly (but not namespace) in CMS 11
				// "everything" depends on EPiServer.dll so it's safe to check against it
				// DataFactory was marked legacy in CMS 10 and removed in CMS 12
				if (dataFactorySymbol != null
					&& startContext.Compilation.HasReferenceAssembly("EPiServer", new Version(10, 0)))
				{
					startContext.RegisterOperationAction(operationContext =>
						DataFacoryAction(operationContext, dataFactorySymbol), OperationKind.Invocation);
				}
			});
		}

		private void DataFacoryAction(OperationAnalysisContext context, INamedTypeSymbol dataFactorySymbol)
		{
			var operation = (IInvocationOperation)context.Operation;

			// TODO: Figure out if I can make this work with extension methods.
			// This method on DataFactory is detected and reported:
			// ContentReference Copy(ContentReference, ContentReference, AccessLevel, AccessLevel, bool)
			// But this extension method when used as a method on DataFactory is not recognized:
			// ContentReference Copy(this IContentRepository, ContentReference, ContentReference)
			// Another OperationKind analysis may be needed and executed after this one, identifying
			// all DataFactory usages but only reporting on those not reported by the code below?

			if (!SymbolEqualityComparer.Default.Equals(operation.TargetMethod?.ContainingType, dataFactorySymbol)
				&& !operation.Syntax.GetText().Lines[0].ToString().Contains("DataFactory.Instance"))
			{
				return;
			}

			switch (operation.TargetMethod.Name)
			{
				case "Copy":
					Report(context, operation, CopyRule, "IContentRepository.Copy");
					break;
				case "CreateLanguageBranch":
					Report(context, operation, CreateLanguageBranchRule, "IContentRepository.CreateLanguageBranch");
					break;
				case "Delete":
					Report(context, operation, DeleteRule, "IContentRepository.Delete");
					break;
				case "DeleteChildren":
					Report(context, operation, DeleteChildrenRule, "IContentRepository.DeleteChildren");
					break;
				case "DeleteLanguageBranch":
					Report(context, operation, DeleteLanguageBranchRule, "IContentRepository.DeleteLanguageBranch");
					break;
				case "DeleteVersion":
					Report(context, operation, DeleteVersionRule, "IContentVersionRepository.Delete");
					break;
				case "FindAllPagesWithCriteria":
					Report(context, operation, FindAllPagesWithCriteriaRule, "IPageCriteriaQueryService.FindAllPagesWithCriteria");
					break;
				case "FindPagesWithCriteria":
					Report(context, operation, FindPagesWithCriteriaRule, "IPageCriteriaQueryService.FindPagesWithCriteria");
					break;
				case "Get":
					Report(context, operation, GetRule, "IContentLoader.Get");
					break;
				case "GetAncestors":
					Report(context, operation, GetAncestorsRule, "IContentLoader.GetAncestors");
					break;
				case "GetBySegment":
					Report(context, operation, GetBySegmentRule, "IContentLoader.GetBySegment");
					break;
				case "GetChildren":
					Report(context, operation, GetChildrenRule, "IContentLoader.GetChildren");
					break;
				case "GetDefault":
					Report(context, operation, GetDefaultRule, "IContentRepository.GetDefault");
					break;
				case "GetDefaultPageData":
					Report(context, operation, GetDefaultPageDataRule, "IContentRepository.GetDefault");
					break;
				case "GetDescendents":
					Report(context, operation, GetDescendentsRule, "IContentLoader.GetDescendents");
					break;
				case "GetItems":
					Report(context, operation, GetItemsRule, "IContentLoader.GetItems");
					break;
				case "GetLanguageBranches":
					Report(context, operation, GetLanguageBranchesRule, "IContentRepository.GetLanguageBranches");
					break;
				case "GetLinksToPages":
					Report(context, operation, GetLinksToPagesRule, "IContentRepository.GetReferencesToContent");
					break;
				case "GetPage":
					Report(context, operation, GetPageRule, "IContentLoader.Get");
					break;
				case "GetProvider":
					Report(context, operation, GetProviderRule, "IContentProviderManager.GetProvider");
					break;
				case "GetReferencesToContent":
					Report(context, operation, GetReferencesToContentRule, "IContentRepository.GetReferencesToContent");
					break;
				case "GetSettingsFromContent":
					Report(context, operation, GetSettingsFromContentRule, "EPiServer.Configuration.Settings.Instance");
					break;
				case "GetSettingsFromPage":
					Report(context, operation, GetSettingsFromPageRule, "EPiServer.Configuration.Settings.Instance");
					break;
				case "GetPages":
					Report(context, operation, GetPagesRule, "IContentLoader.GetItems");
					break;
				case "GetParents":
					Report(context, operation, GetParentsRule, "IContentLoader.GetAncestors");
					break;
				case "HasEntryPointChild":
					Report(context, operation, HasEntryPointRule, "IContentProviderManager.HasEntryPointChild");
					break;
				case "IsCapabilitySupported":
					Report(context, operation, IsCapabilitySupportedRule, "IContentProviderManager.IsCapabilitySupported");
					break;
				case "IsWastebasket":
					Report(context, operation, IsWastebasketRule, "IContentProviderManager.IsWastebasket");
					break;
				case "ListDelayedPublish":
					Report(context, operation, ListDelayedPublishRule, "IContentRepository.ListDelayedPublish");
					break;
				case "ListPublishedVersions":
					Report(context, operation, ListPublishedVersionsRule, "IContentVersionRepository.ListPublished");
					break;
				case "ListVersions":
					Report(context, operation, ListVersionsRule, "IContentVersionRepository.List");
					break;
				case "LoadPublishedVersion":
					Report(context, operation, LoadPublishedVersionRule, "IContentVersionRepository.LoadPublished");
					break;
				case "LoadVersion":
					Report(context, operation, LoadVersionRule, "IContentVersionRepository.Load");
					break;
				case "Move":
					Report(context, operation, MoveRule, "IContentRepository.Move");
					break;
				case "MoveToWastebasket":
					Report(context, operation, MoveToWastbasketRule, "IContentRepository.MoveToWastebasket");
					break;
				case "ResetCounters":
					Report(context, operation, ResetCountersRule, "IContentProviderManager.ProviderMap.Iterate");
					break;
				case "ResolveContentFolder":
					Report(context, operation, ResolveContentFolderRule, null);
					break;
				case "ResolvePageFolder":
					Report(context, operation, ResolvePageFolderRule, null);
					break;
				case "Save":
					Report(context, operation, SaveRule, "IContentRepository.Save");
					break;
				case "TryGet":
					Report(context, operation, TryGetRule, "IContentLoader.TryGet");
					break;
			}
		}

		private void Report(OperationAnalysisContext context, IInvocationOperation operation,
			DiagnosticDescriptor rule, string useThis)
		{
			if (useThis != null)
			{
				context.ReportDiagnostic(Diagnostic.Create(rule, operation.Syntax.GetLocation(),
					useThis, $"DataFactory.{operation.TargetMethod.Name}"));
			}
			else
			{
				context.ReportDiagnostic(Diagnostic.Create(rule, operation.Syntax.GetLocation()));
			}
		}
	}
}
