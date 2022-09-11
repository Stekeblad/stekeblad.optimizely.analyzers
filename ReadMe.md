# Analyzers For Optimizely CMS

## Avoid some easy misstakes when building Optimizely sites with these analyzers.

If you have been working with Optimizely CMS (previously Episerver) you have
probably at least once forgotten that little "virtual" keyword when adding a new property
to a page or block type. You want know that something is wrong until you try to
run the project. That is until now!

![https://github.com/stekeblad/stekeblad.optimizely.analyzers/doc/ExampleWarnings.jpg](Example of analyzer warnings and a codefix preview)

Analyzers For Optimizely CMS will also warn about when an attribute
is expected but not found. (For example on Initializable modules.)

## No hard dependencies

Analyzers For Optimizely CMS will not error if no Optimizely packages are
present, it does not require any specific version.
In fact, the analyzers SOA1001 to SOA1005 have been tested to work
with CMS 12.6, 11.12 and even 8.11! While newer analyzers may work on very old versions,
focus will only be on supporting CMS 11 and newer.

## Learn from the analyzers

My goal is for all analyzers to not just tell you what you did wrong but also
explain why its wrong, what may be happening in the background and how to fix
the issue if a fix is not offered. Just click on the Analyzer id in the Error List
tool window, the popup when hovering over the warning or code fix preview in Visual Studio to learn more.
See for example the help documentation for
[SOA1002](https://github.com/stekeblad/stekeblad.optimizely.analyzers/SOA1002)
and [SOA1003](https://github.com/stekeblad/stekeblad.optimizely.analyzers/SOA1003)

## How to get

Analyzers For Optimizely CMS will in a few days be available both as an
extension to Visual Studio for use in all your projects and as a NuGet package
to bring the analyzer to everyone working on a project!

## Currently available analyzers

- Content types without ContentTypeAttribute or the attribute is missing the GUID property
- Properties in content types that lacks the virtual keyword
- Incomplete definition of Initializable modules
(needs to both implement interface and decorate with attribute)

More will come!

## Contribute

Contributions of different kinds are wellcome!
- Suggestions for new analyzers and fixes
- Implementations of new analyzers, fixes and testcases
- Improvements to analyzer documentation