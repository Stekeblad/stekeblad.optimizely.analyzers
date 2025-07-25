﻿## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1001 | Defining content | Warning | [Use ContentTypeAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1001.md)
SOA1002 | Defining content | Warning | [Add GUID parameter to ContentTypeAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1002.md)
SOA1003 | Defining content | Warning | [Public non-static properties must be declared virtual](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1003.md)
SOA1004 | Initialization modules | Warning | [Initializable modules needs an attribute to be discovered](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1004.md)
SOA1005 | Initialization modules | Warning | [Implement the interface IInitializableModule or IConfigurableModule](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1005.md)

## Release 1.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1006 | Scheduled jobs | Warning | [Inherit from ScheduledJobBase](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1006.md)
SOA1007 | Scheduled jobs | Warning | [Decorate with ScheduledPluginAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1007.md)
SOA1008 | Scheduled jobs | Info | [ScheduledPluginAttribute has no GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1008.md)

## Release 1.2.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1101 | DataFactory | Warning | [Use 'IContentRepository.Copy'](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1101.md)
SOA1102 | DataFactory | Warning | [Use 'IContentRepository.CreateLanguageBranch'](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1102.md)
SOA1103 | DataFactory | Warning | [Use 'IContentRepository.Delete'](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1103.md)
SOA1104 | DataFactory | Warning | [Use IContentRepository.DeleteChildren](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1104.md)
SOA1105 | DataFactory | Warning | [Use IContentRepository.DeleteLanguageBranch](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1105.md)
SOA1106 | DataFactory | Warning | [Use IContentVersionRepository.Delete](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1106.md)
SOA1107 | DataFactory | Warning | [Use IPageCriteriaQueryService.FindAllPagesWithCriteria](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1107.md)
SOA1108 | DataFactory | Warning | [Use IPageCriteriaQueryService.FindPagesWithCriteria](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1108.md)
SOA1109 | DataFactory | Warning | [Use IContentLoader.Get](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1109.md)
SOA1110 | DataFactory | Warning | [Use IContentLoader.GetAncestors](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1110.md)
SOA1111 | DataFactory | Warning | [Use IContentLoader.GetBySegment](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1111.md)
SOA1112 | DataFactory | Warning | [Use IContentLoader.GetChildren](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1112.md)
SOA1113 | DataFactory | Warning | [Use IContentRepository.GetDefault](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1113.md)
SOA1114 | DataFactory | Warning | [Use IContentRepository.GetDefault](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1114.md)
SOA1115 | DataFactory | Warning | [Use IContentLoader.GetDescendents](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1115.md)
SOA1116 | DataFactory | Warning | [Use IContentLoader.GetItems](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1116.md)
SOA1117 | DataFactory | Warning | [Use IContentRepository.GetLanguageBranches](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1117.md)
SOA1118 | DataFactory | Warning | [Use IContentRepository.GetReferencesToContent](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1118.md)
SOA1119 | DataFactory | Warning | [Use IContentLoader.Get](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1119.md)
SOA1120 | DataFactory | Warning | [Use IContentProviderManager.GetProvider](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1120.md)
SOA1121 | DataFactory | Warning | [Use IContentRepository.GetReferencesToContent](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1121.md)
SOA1122 | DataFactory | Warning | [Use EPiServer.Configuration.Settings.Instance](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1122.md)
SOA1123 | DataFactory | Warning | [Use EPiServer.Configuration.Settings.Instance](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1123.md)
SOA1124 | DataFactory | Warning | [Use IContentLoader.GetItems](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1124.md)
SOA1125 | DataFactory | Warning | [Use IContentLoader.GetAncestors](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1125.md)
SOA1126 | DataFactory | Warning | [Use IContentProviderManager.HasEntryPoint](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1126.md)
SOA1127 | DataFactory | Warning | [Use IContentProviderManager.IsCapabilitySupported](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1127.md)
SOA1128 | DataFactory | Warning | [Use IContentProviderManager.IsWastebasket](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1128.md)
SOA1129 | DataFactory | Warning | [Use IContentRepository.ListDelayedPublish](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1129.md)
SOA1130 | DataFactory | Warning | [Use IContentVersionRepository.ListPublished](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1130.md)
SOA1131 | DataFactory | Warning | [Use IContentVersionRepository.List](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1131.md)
SOA1132 | DataFactory | Warning | [Use IContentVersionRepository.LoadPublished](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1132.md)
SOA1133 | DataFactory | Warning | [Use IContentVersionRepository.Load](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1133.md)
SOA1134 | DataFactory | Warning | [Use IContentRepository.Move](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1134.md)
SOA1135 | DataFactory | Warning | [Use IContentRepository.MoveToWastbasket](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1135.md)
SOA1136 | DataFactory | Warning | [Use IContentProviderManager.ProviderMap.Iterate](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1136.md)
SOA1137 | DataFactory | Warning | [ResolveContentFolder is no longer supported](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1137.md)
SOA1138 | DataFactory | Warning | [ResolvePageFolder is no longer supported](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1138.md)
SOA1139 | DataFactory | Warning | [Use IContentRepository.Save](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1139.md)
SOA1140 | DataFactory | Warning | [Use IContentLoader.TryGet](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1140.md)

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
SOA1001 | DefiningContent | Warning | Defining content | Warning | [Use ContentTypeAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1001.md)
SOA1002 | DefiningContent | Warning | Defining content | Warning | [Add GUID parameter to ContentTypeAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1002.md)
SOA1003 | DefiningContent | Error | Defining content | Warning | [Public non-static properties must be declared virtual](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1003.md)
SOA1004 | InitializationModules | Warning | Initialization modules | Warning | [Initializable modules needs an attribute to be discovered](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1004.md)
SOA1005 | InitializationModules | Warning | Initialization modules | Warning | [Implement the interface IInitializableModule or IConfigurableModule](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1005.md)
SOA1006 | ScheduledJobs | Warning | Scheduled jobs | Warning | [Inherit from ScheduledJobBase](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1006.md)
SOA1007 | ScheduledJobs | Warning | Scheduled jobs | Warning | [Decorate with ScheduledPluginAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1007.md)
SOA1008 | ScheduledJobs | Info | Scheduled jobs | Info | [ScheduledPluginAttribute has no GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1008.md)

## Release 1.3.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1009 | DefiningContent | Error | [Multiple content types must not share GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1009.md)
SOA1010 | DefiningContent | Error | [ContentTypeAttribute has an invalid GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1010.md)
SOA1011 | ScheduledJobs | Error | [ScheduledPluginAttribute has an invalid GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1011.md)
SOA1012 | ScheduledJobs | Error | [Multiple scheduled jobs must not share GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1012.md)
SOA1013 | DefiningContent | Warning | [Only one of SelectOne, SelectMany, AutoSuggestSelection (or deriving attributes) should be used on a property](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1013.md)
SOA1014 | DefiningContent | Warning | [Properties decorated with one of the attributes SelectOne, SelectMany or AutoSuggestSelection (or deriving attributes) must be of the type string or int for the selection to work](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1014.md)
SOA1015 | DefiningContent | Warning | [Attribute is missing the SelectionFactoryType parameter](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1015.md)
SOA1016 | DefiningContent | Warning | [SelectionFactoryType does not implement ISelectionFactory or is declared abstract](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1016.md)
SOA1017 | DefiningContent | Warning | [SelectionFactoryType does not implement ISelectionQuery or is declared abstract](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1017.md)

## Release 1.3.1

### Changed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1014 | DefiningContent | Warning | Title was changed to [Type may not work with the attributes SelectOne, SelectMany or AutoSuggestSelection (or deriving attributes)](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1014.md) and `enum`s is now considered an allowed type

## Release 1.4.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1018 | BadMethods | Information | [Cast UrlBuilder to string instead of calling ToString method](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1018.md)

## Release 1.5.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1019 | DefiningContent | Warning | [Initialize property inside SetDefaultValues](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1019.md) instead of using property initialization

### Changed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1014 | DefiningContent | Warning | [Type may not work with the attributes SelectOne, SelectMany or AutoSuggestSelection (or deriving attributes)](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1014.md) now supports nullable int, string and enums

## Release 1.6.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1020 | DefiningContent | Warning | [Missing MediaDescriptorAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1020.md)
SOA1021 | DefiningContent | Warning | [Unexpected usage of MediaDescriptorAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1021.md)
SOA1022 | DefiningContent | Warning | [File extension associated with multiple content types](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1022.md)
SOA1023 | DefiningContent | Information | [Unexpected base type for file extension](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1023.md)
SOA1024 | DefiningContent | Error | [Missing arguments on MediaDescriptorAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1024.md)

## Release 1.7.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1025 | InitializationModules | Error | [Dependencies to initializable modules must be initializable modules](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1025.md)
SOA1026 | BadMethods | Warning | [Use IContentRepository.GetDefault\<T>()](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1026.md)
SOA1027 | DefiningContent | Hidden | [Convert to full Optimizely property](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1027.md)

## Release 1.8.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1028 | BadMethods | Warning | [Block type is cast to IContent](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1028.md)
SOA1029 | BadMethods | Warning | [Don't manually access content from a ContentAreaItem](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1029.md)
SOA1030 | DefiningContent | Warning | [Missing PropertyDefinitionTypePlugInAttribute on custom property definition](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1030.md)
SOA1031 | DefiningContent | Warning | [Missing or incompatible base class for property definition](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1031.md)
SOA1032 | DefiningContent | Warning | [Add GUID to PropertyDefinitionTypePlugInAttribute](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1032.md)
SOA1033 | DefiningContent | Error | [PropertyDefinitionTypePlugInAttribute has an invalid GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1033.md)
SOA1034 | DefiningContent | Error | [Multiple PropertyDefinitionTypePlugInAttributes must not share GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1034.md)

### Changed Rules

Rule ID | Notes
--------|------
SOA1009 | Updated the message for [Multiple content types must not share GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1009.md) to be on the same format as the new SOA1034 added in this release
SOA1012 | Updated the message for [Multiple scheduled jobs must not share GUID](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1012.md) to be on the same format as the new SOA1034 added in this release

## Release 1.9.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SOA1035 | DefiningContent | Error | [Content type constructor has parameters](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1035.md)
SOA1036 | DefiningContent | Warning | [Content property initialized inside constructor](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1036.md)
SOA1037 | DefiningContent | Error | [Don't register abstract PropertyDefinitionTypePlugIn](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1037.md)
SOA1038 | DefiningContent | Warning | [Don't register abstract ContentType](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1038.md)
SOA1039 | Scheduled jobs  | Error | [Don't register abstract ScheduledJob](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1039.md)
SOA1040 | InitializationModules | Error | [Don't register abstract Initialization module](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1040.md)
SOA1041 | Scheduled job | Warning | [ScheduledJob has no name](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1041.md)