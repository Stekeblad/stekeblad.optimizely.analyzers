# Analyzers For Optimizely CMS

## Avoid some easy mistakes when building Optimizely sites with these analyzers.

If you have been working with Optimizely CMS (previously Episerver) you have
probably at least once forgotten that little "virtual" keyword when adding a new property
to a page or block type. You wont know that something is wrong until you try to
run the project. That was true, until now!

![Example of analyzer warnings and a codefix preview](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/images/ExampleWarnings.jpg)

Analyzers For Optimizely CMS will also warn about when an attribute
is expected but not found. (For example on Initializable modules, content types, scheduled jobs.)
Read more on available analyzers below.

## No hard dependencies

Analyzers For Optimizely CMS will not error if no Optimizely packages are
present, it does not require any specific version.
In fact, the analyzers SOA1001 to SOA1005 have been tested to work
with CMS 12.6, 11.12 and even 8.11! While analyzers may work on very old versions,
focus will only be on supporting CMS 11 and later.

## Learn from the analyzers

My goal is for all analyzers to not just tell you what you did wrong but also
explain why its wrong, what may be happening in the background and how to fix
the issue if a fix is not offered as a code fix. Just click on the Analyzer id in the Error List
tool window, the popup when hovering over the warning or code fix preview in Visual Studio to learn more.

## How to get

You can get Analyzers For Optimizely CMS both as an
[extension to Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=Stekeblad.optianalyzers)
for use with all your projects and as a
[NuGet package](https://nuget.optimizely.com/package/?id=Stekeblad.Optimizely.Analyzers)
to bring the analyzer to everyone working on a project.

The nuget options is the recommended one. Installing analyzers as an extension comes
with some limitations causing some analyzers not to work,
one example being the checks for reused guids.

## Available analyzers

Here are a quick overview of some of the things the analyzers test for:

- Content types without inheriting base class or
being decorated with ContentTypeAttribute, the attribute is missing
the GUID property, the GUID is invalid or used on multiple content types
- Properties in content types that lacks the virtual keyword
- Incomplete definition of Initializable modules
(needs to both implement interface and be decorated with attribute)
- Incomplete definition of Scheduled jobs (needs both base type and attribute)
- Incorrect usage of selection factories (e.g. SelectOne/SelectManyAttribute)
- Usage of the legacy DataFactory class (Only CMS version 10+)

More will come!

[List of available analyzers](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/src/Analyzers/AnalyzerReleases.Shipped.md)

## Contribute

Do you want to report a bug or suggest a feature?

Do you see something in the documentation that can be improved?

Do you want to help creating new analyzers, tests or writing documentation?
[Create an issue](https://github.com/Stekeblad/stekeblad.optimizely.analyzers/issues/new/choose)
and let us take it from there.