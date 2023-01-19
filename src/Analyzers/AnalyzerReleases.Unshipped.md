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