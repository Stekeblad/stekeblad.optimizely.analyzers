using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.DataFactory
{
	public partial class DataFactoryAnalyzer
	{
		public static readonly LocalizableString Title = "DataFactoy is considered legacy";
		internal static readonly LocalizableString MessageFormat = "Use '{0}' instead of '{1}'";
		internal const string Category = Constants.Categories.DataFactory;

		private static DiagnosticDescriptor CreateRule(string id) =>
			new DiagnosticDescriptor(id, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
				true, helpLinkUri: HelpUrl(id));

		public const string CopyDiagnosticId = "SOA1101";
		public const string CreateLanguageBranchDiagnosticId = "SOA1102";
		public const string DeleteDiagnosticId = "SOA1103";
		public const string DeleteChildrenDiagnosticId = "SOA1104";
		public const string DeleteLanguageBranchDiagnosticId = "SOA1105";
		public const string DeleteVersionDiagnosticId = "SOA1106";
		public const string FindAllPagesWithCriteriaDiagnosticId = "SOA1107";
		public const string FindPagesWithCriteriaDiagnosticId = "SOA1108";
		public const string GetDiagnosticId = "SOA1109";
		public const string GetAncestorsDiagnosticId = "SOA1110";
		public const string GetBySegmentDiagnosticId = "SOA1111";
		public const string GetChildrenDiagnosticId = "SOA1112";
		public const string GetDefaultDiagnosticId = "SOA1113";
		public const string GetDefaultPageDataDiagnosticId = "SOA1114";
		public const string GetDescendentsDiagnosticId = "SOA1115";
		public const string GetItemsDiagnosticId = "SOA1116";
		public const string GetLanguageBranchesDiagnosticId = "SOA1117";
		public const string GetLinksToPagesDiagnosticId = "SOA1118";
		public const string GetPageDiagnosticId = "SOA1119";
		public const string GetProviderDiagnosticId = "SOA1120";
		public const string GetReferencesToContentDiagnosticId = "SOA1121";
		public const string GetSettingsFromContentDiagnosticId = "SOA1122";
		public const string GetSettingsFromPageDiagnosticId = "SOA1123";
		public const string GetPagesDiagnosticId = "SOA1124";
		public const string GetParentsDiagnosticId = "SOA1125";
		public const string HasEntryPointDiagnosticId = "SOA1126";
		public const string IsCapabilitySupportedDiagnosticId = "SOA1127";
		public const string IsWastebasketDiagnosticId = "SOA1128";
		public const string ListDelayedPublishDiagnosticId = "SOA1129";
		public const string ListPublishedVersionsDiagnosticId = "SOA1130";
		public const string ListVersionsDiagnosticId = "SOA1131";
		public const string LoadPublishedVersionDiagnosticId = "SOA1132";
		public const string LoadVersionDiagnosticId = "SOA1133";
		public const string MoveDiagnosticId = "SOA1134";
		public const string MoveToWastbasketDiagnosticId = "SOA1135";
		public const string ResetCountersDiagnosticId = "SOA1136";
		public const string ResolveContentFolderDiagnosticId = "SOA1137";
		public const string ResolvePageFolderDiagnosticId = "SOA1138";
		public const string SaveDiagnosticId = "SOA1139";
		public const string TryGetDiagnosticId = "SOA1140";

		internal static DiagnosticDescriptor CopyRule = CreateRule(CopyDiagnosticId);
		internal static DiagnosticDescriptor CreateLanguageBranchRule = CreateRule(CreateLanguageBranchDiagnosticId);
		internal static DiagnosticDescriptor DeleteRule = CreateRule(DeleteDiagnosticId);
		internal static DiagnosticDescriptor DeleteChildrenRule = CreateRule(DeleteChildrenDiagnosticId);
		internal static DiagnosticDescriptor DeleteLanguageBranchRule = CreateRule(DeleteLanguageBranchDiagnosticId);
		internal static DiagnosticDescriptor DeleteVersionRule = CreateRule(DeleteVersionDiagnosticId);
		internal static DiagnosticDescriptor FindAllPagesWithCriteriaRule = CreateRule(FindAllPagesWithCriteriaDiagnosticId);
		internal static DiagnosticDescriptor FindPagesWithCriteriaRule = CreateRule(FindPagesWithCriteriaDiagnosticId);
		internal static DiagnosticDescriptor GetRule = CreateRule(GetDiagnosticId);
		internal static DiagnosticDescriptor GetAncestorsRule = CreateRule(GetAncestorsDiagnosticId);
		internal static DiagnosticDescriptor GetBySegmentRule = CreateRule(GetBySegmentDiagnosticId);
		internal static DiagnosticDescriptor GetChildrenRule = CreateRule(GetChildrenDiagnosticId);
		internal static DiagnosticDescriptor GetDefaultRule = CreateRule(GetDefaultDiagnosticId);
		internal static DiagnosticDescriptor GetDefaultPageDataRule = CreateRule(GetDefaultPageDataDiagnosticId);
		internal static DiagnosticDescriptor GetDescendentsRule = CreateRule(GetDescendentsDiagnosticId);
		internal static DiagnosticDescriptor GetItemsRule = CreateRule(GetItemsDiagnosticId);
		internal static DiagnosticDescriptor GetLanguageBranchesRule = CreateRule(GetLanguageBranchesDiagnosticId);
		internal static DiagnosticDescriptor GetLinksToPagesRule = CreateRule(GetLinksToPagesDiagnosticId);
		internal static DiagnosticDescriptor GetPageRule = CreateRule(GetPageDiagnosticId);
		internal static DiagnosticDescriptor GetProviderRule = CreateRule(GetProviderDiagnosticId);
		internal static DiagnosticDescriptor GetReferencesToContentRule = CreateRule(GetReferencesToContentDiagnosticId);
		internal static DiagnosticDescriptor GetSettingsFromContentRule = CreateRule(GetSettingsFromContentDiagnosticId);
		internal static DiagnosticDescriptor GetSettingsFromPageRule = CreateRule(GetSettingsFromPageDiagnosticId);
		internal static DiagnosticDescriptor GetPagesRule = CreateRule(GetPagesDiagnosticId);
		internal static DiagnosticDescriptor GetParentsRule = CreateRule(GetParentsDiagnosticId);
		internal static DiagnosticDescriptor HasEntryPointRule = CreateRule(HasEntryPointDiagnosticId);
		internal static DiagnosticDescriptor IsCapabilitySupportedRule = CreateRule(IsCapabilitySupportedDiagnosticId);
		internal static DiagnosticDescriptor IsWastebasketRule = CreateRule(IsWastebasketDiagnosticId);
		internal static DiagnosticDescriptor ListDelayedPublishRule = CreateRule(ListDelayedPublishDiagnosticId);
		internal static DiagnosticDescriptor ListPublishedVersionsRule = CreateRule(ListPublishedVersionsDiagnosticId);
		internal static DiagnosticDescriptor ListVersionsRule = CreateRule(ListVersionsDiagnosticId);
		internal static DiagnosticDescriptor LoadPublishedVersionRule = CreateRule(LoadPublishedVersionDiagnosticId);
		internal static DiagnosticDescriptor LoadVersionRule = CreateRule(LoadVersionDiagnosticId);
		internal static DiagnosticDescriptor MoveRule = CreateRule(MoveDiagnosticId);
		internal static DiagnosticDescriptor MoveToWastbasketRule = CreateRule(MoveToWastbasketDiagnosticId);
		internal static DiagnosticDescriptor ResetCountersRule = CreateRule(ResetCountersDiagnosticId);
		internal static DiagnosticDescriptor ResolveContentFolderRule =
			new DiagnosticDescriptor(ResolveContentFolderDiagnosticId, Title, "ResolveContentFolder is no longer supported",
				Category, DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(ResolveContentFolderDiagnosticId));
		internal static DiagnosticDescriptor ResolvePageFolderRule =
			new DiagnosticDescriptor(ResolvePageFolderDiagnosticId, Title, "ResolvePageFolder is no longer supported",
				Category, DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(ResolvePageFolderDiagnosticId));
		internal static DiagnosticDescriptor SaveRule = CreateRule(SaveDiagnosticId);
		internal static DiagnosticDescriptor TryGetRule = CreateRule(TryGetDiagnosticId);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(
					CopyRule,
					CreateLanguageBranchRule,
					DeleteRule,
					DeleteChildrenRule,
					DeleteLanguageBranchRule,
					DeleteVersionRule,
					FindAllPagesWithCriteriaRule,
					FindPagesWithCriteriaRule,
					GetRule,
					GetAncestorsRule,
					GetBySegmentRule,
					GetChildrenRule,
					GetDefaultRule,
					GetDefaultPageDataRule,
					GetDescendentsRule,
					GetItemsRule,
					GetLanguageBranchesRule,
					GetLinksToPagesRule,
					GetPageRule,
					GetProviderRule,
					GetReferencesToContentRule,
					GetSettingsFromContentRule,
					GetSettingsFromPageRule,
					GetPagesRule,
					GetParentsRule,
					HasEntryPointRule,
					IsCapabilitySupportedRule,
					IsWastebasketRule,
					ListDelayedPublishRule,
					ListPublishedVersionsRule,
					ListVersionsRule,
					LoadPublishedVersionRule,
					LoadVersionRule,
					MoveRule,
					MoveToWastbasketRule,
					ResetCountersRule,
					ResolveContentFolderRule,
					ResolvePageFolderRule,
					SaveRule,
					TryGetRule
				);
			}
		}
	}
}
