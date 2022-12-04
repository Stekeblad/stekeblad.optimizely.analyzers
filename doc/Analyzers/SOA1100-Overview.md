# SOA1100 Overview

This is an overview of the SOA1101-1199 series of analyzers.

## DataFactory is considered legacy

DataFactory was marked as legacy in CMS 10 and removed in CMS 12.

There are no fixes offered for this series of analyzers because
the fix requires instances of various services that may be obtained
in different ways, the complexity of testing all scenrios for if
they are present at the fix location and that the codefix would have
it very difficult at picking the best way to get an instance for you.

So, instead of offering a direct code fix, the documentation for all
the SOA11XX-analyzers have examples of a fix.

Just a few ways to get instances of services:

- Dependency injection in the constructor
- A Injected\<T> property
- Calling ServiceLocator.Current.GetInstance\<T>()
- Provide to the method as a paramteter

The SOA11XX examples will use ServiceLocator because it keeps the examples short,
however its recommended to use dependency injection when possible.

## DataFactory method analyzers

- [Copy](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1101.md)
- [CreateLanguageBranch](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1102.md)
- [Delete](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1103.md)
- [DeleteChildren](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1104.md)
- [DeleteLanguageBranch](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1105.md)
- [DeleteVersion](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1106.md)
- [FindAllPagesWithCriteria](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1107.md)
- [FindPagesWithCriteria](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1108.md)
- [Get](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1109.md)
- [GetAncestors](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1110.md)
- [GetBySegment](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1111.md)
- [GetChildren](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1112.md)
- [GetDefault](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1113.md)
- [GetDefaultPageData](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1114.md)
- [GetDescendents](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1115.md)
- [GetItems](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1116.md)
- [GetLanguageBranches](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1117.md)
- [GetLinksToPages](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1118.md)
- [GetPage](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1119.md)
- [GetProvider](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1120.md)
- [GetReferencesToContent](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1121.md)
- [GetSettingsFromContent](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1122.md)
- [GetSettingsFromPage](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1123.md)
- [GetPages](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1124.md)
- [GetParents](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1125.md)
- [HasEntryPoint](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1126.md)
- [IsCapabilitySupported](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1127.md)
- [IsWastebasket](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1128.md)
- [ListDelayedPublish](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1129.md)
- [ListPublishedVersions](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1130.md)
- [ListVersions](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1131.md)
- [LoadPublishedVersion](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1132.md)
- [LoadVersion](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1133.md)
- [Move](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1134.md)
- [MoveToWastbasket](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1135.md)
- [ResetCounters](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1136.md)
- [ResolveContentFolder](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1137.md)
- [ResolvePageFolder](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1138.md)
- [Save](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1139.md)
- [TryGet](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/1140.md)