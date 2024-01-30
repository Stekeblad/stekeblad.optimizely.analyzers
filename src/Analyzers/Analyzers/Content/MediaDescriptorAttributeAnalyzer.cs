using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.Content
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MediaDescriptorAttributeAnalyzer : MyDiagnosticAnalyzerBase
    {
        public const string UseMediaDescriptorDiagnosticId = "SOA1020";
        public const string UseMediaDescriptorTitle = "Missing MediaDescriptorAttribute";
        internal const string UseMediaDescriptorMessageFormat =
            "Add MediaDescriptorAttribute to {0} and define file extensions to be mapped to this type, omitting the attribute allows all file extensions";

        internal static DiagnosticDescriptor UseMediaDescriptorRule =
            new DiagnosticDescriptor(UseMediaDescriptorDiagnosticId, UseMediaDescriptorTitle,
                UseMediaDescriptorMessageFormat, Constants.Categories.DefiningContent,
                DiagnosticSeverity.Warning, true, helpLinkUri: HelpUrl(UseMediaDescriptorDiagnosticId));

        public const string MediaDescriptorNotExpectedDiagnosticId = "SOA1021";
        public const string MediaDescriptorNotExpectedTitle = "Unexpected usage of MediaDescriptorAttribute";
        internal const string MediaDescriptorNotExpectedMessageFormat =
            "MediaDescriptorAttribute should only be on classes implementing IContentMedia, like classes inheriting from MediaData, ImageData or VideoData";

        internal static DiagnosticDescriptor MediaDescriptorNotExpectedRule =
            new DiagnosticDescriptor(MediaDescriptorNotExpectedDiagnosticId, MediaDescriptorNotExpectedTitle,
                MediaDescriptorNotExpectedMessageFormat, Constants.Categories.DefiningContent,
                DiagnosticSeverity.Warning, true, HelpUrl(MediaDescriptorNotExpectedDiagnosticId));

        public const string ExtensionReusedDiagnosticId = "SOA1022";
        public const string ExtensionReusedTitle = "File extension associated with multiple content types";
        internal const string ExtensionReusedMessageFormat =
            "The file extension {0} is associated with the following content types: {1}. You should only associate an extension with one content type.";

        internal static DiagnosticDescriptor ExtensionReusedRule =
            new DiagnosticDescriptor(ExtensionReusedDiagnosticId, ExtensionReusedTitle,
                ExtensionReusedMessageFormat, Constants.Categories.DefiningContent,
                DiagnosticSeverity.Warning, true, HelpUrl(ExtensionReusedDiagnosticId));

        public const string UnexpectedMediaBaseTypeDiagnosticId = "SOA1023";
        public const string UnexpectedMediaBaseTypeTitle = "Unexpected base type for file extension";
        internal const string UnexpectedMediaBaseTypeMessageFormat =
            "The file extension {0} was not expected on this content type, consider registering it with a type {1}";

        internal static DiagnosticDescriptor UnexpectedMediaBaseTypeRule =
            new DiagnosticDescriptor(UnexpectedMediaBaseTypeDiagnosticId, UnexpectedMediaBaseTypeTitle,
                UnexpectedMediaBaseTypeMessageFormat, Constants.Categories.DefiningContent,
                DiagnosticSeverity.Info, true, HelpUrl(UnexpectedMediaBaseTypeDiagnosticId));

        public const string MissingMediaDescriptorArgumentsDiagnosticId = "SOA1024";
        public const string MissingMediaDescriptorArgumentsTitle = "Missing arguments on MediaDescriptorAttribute";
        internal const string MissingMediaDescriptorArgumentsMessageFormat =
            "The MediaDescriptorAttribute is missing a required argument, add ExtensionString or Extensions";

        internal static DiagnosticDescriptor MissingMediaDescriptorArgumentsRule =
            new DiagnosticDescriptor(MissingMediaDescriptorArgumentsDiagnosticId, MissingMediaDescriptorArgumentsTitle,
                MissingMediaDescriptorArgumentsMessageFormat, Constants.Categories.DefiningContent,
                DiagnosticSeverity.Error, true, HelpUrl(MissingMediaDescriptorArgumentsDiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    UseMediaDescriptorRule,
                    MediaDescriptorNotExpectedRule,
                    ExtensionReusedRule,
                    UnexpectedMediaBaseTypeRule,
                    MissingMediaDescriptorArgumentsRule);
            }
        }

#pragma warning disable RS1026 // Enable concurrent execution
        public override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            //context.EnableConcurrentExecution(); // Not thread safe!

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol contentMediaSymbol = startContext.Compilation.GetTypeByMetadataName(
                      "EPiServer.Core.IContentMedia");
                INamedTypeSymbol mediaDescriptorAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
                        "EPiServer.Framework.DataAnnotations.MediaDescriptorAttribute");
                INamedTypeSymbol imageDataSymbol = startContext.Compilation.GetTypeByMetadataName(
                    "EPiServer.Core.ImageData");
                INamedTypeSymbol videoDataSymbol = startContext.Compilation.GetTypeByMetadataName(
                    "EPiServer.Core.VideoData");

                if (mediaDescriptorAttributeSymbol != null && imageDataSymbol != null)
                {
                    List<(string Extension, INamedTypeSymbol Type, AttributeData Attribute)> extensionTypeAttributeList =
                        new List<(string Extension, INamedTypeSymbol Type, AttributeData Attribute)>();

                    startContext.RegisterSymbolAction(
                        nodeContext => AnalyzeNamedTypeSymbol(nodeContext, contentMediaSymbol,
                            mediaDescriptorAttributeSymbol, extensionTypeAttributeList),
                        SymbolKind.NamedType);

                    startContext.RegisterCompilationEndAction(endContext =>
                        Summarize(endContext, imageDataSymbol, videoDataSymbol, extensionTypeAttributeList));
                }
            });
        }

        private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context,
            INamedTypeSymbol contentMediaSymbol,
            INamedTypeSymbol mediaDescriptorAttributeSymbol,
            List<(string Extension, INamedTypeSymbol Type, AttributeData Attribute)> extensionTypeAttributeList)
        {
            var analyzedSymbol = context.Symbol as INamedTypeSymbol;

            //Check if auto-generated, abstract or not a class.
            if (analyzedSymbol?.IsImplicitlyDeclared != false
                || analyzedSymbol.TypeKind != TypeKind.Class
                || analyzedSymbol.IsAbstract)
            {
                return;
            }

            // Get MediaDescriptor attribute
            analyzedSymbol.TryGetAttributesDerivedFrom(mediaDescriptorAttributeSymbol, out var mediaDescriptorAttributes);

            bool implementsIContentMedia = analyzedSymbol.AllInterfaces.Contains(contentMediaSymbol);

            // First, scenarios when analyzedSymbol does not implement IContentMedia interface
            if (!implementsIContentMedia)
            {
                // If no attribute and interface not implemented, all OK, return
                if (mediaDescriptorAttributes is null)
                {
                    return;
                }
                else
                {
                    foreach (var attribute in mediaDescriptorAttributes)
                    {
                        var diagnostic = Diagnostic.Create(MediaDescriptorNotExpectedRule, attribute.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            if (mediaDescriptorAttributes is null)
            {
                // interface implemented but no attribute
                var diagnostic = Diagnostic.Create(UseMediaDescriptorRule,
                        analyzedSymbol.Locations[0],
                        analyzedSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                // returning as later checks only applies when attributes exists
                return;
            }

            // Collect information on file extensions present in the attribute(s) and build a list of extension, class and attribute
            foreach (var attribute in mediaDescriptorAttributes)
            {
                foreach (var extension in GetAllExtensionsFromAttribute(attribute, context))
                {
                    extensionTypeAttributeList.Add((extension, analyzedSymbol, attribute));
                }
            }
        }

        private void Summarize(CompilationAnalysisContext context,
            INamedTypeSymbol imageDataSymbol,
            INamedTypeSymbol videoDataSymbol,
            List<(string Extension, INamedTypeSymbol Type, AttributeData Attribute)> extensionTypeAttributeList)
        {
            foreach (var typesAttributesWithExtension in extensionTypeAttributeList.GroupBy(x => x.Extension))
            {
                if (typesAttributesWithExtension.Count() > 1)
                {
                    // More than one attribute contains this extension.
                    // It's allowed to have multiple MediaDescriptorAttribute on a class,
                    // only diagnose if the extension appear on more than one class.
                    if (typesAttributesWithExtension.Select(x => x.Type).Distinct(SymbolEqualityComparer.Default).Count() == 1)
                        continue;

                    var typeNames = string.Join(", ", typesAttributesWithExtension.Select(x => x.Type.Name).OrderBy(x => x).Distinct());
                    foreach (var typeAttr in typesAttributesWithExtension)
                    {
                        var diagnostic = Diagnostic.Create(ExtensionReusedRule,
                        FindExtensionLocationInAttribute(ImmutableArray.Create(typeAttr.Attribute), typesAttributesWithExtension.Key),
                        typesAttributesWithExtension.Key,
                        typeNames);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else
                {
                    // If only one occurrence of the extension was found, let's try to see if it is on a type inheriting from
                    // the right base class
                    INamedTypeSymbol type = typesAttributesWithExtension.First().Type;
                    switch (GetFileFormatKind(typesAttributesWithExtension.Key))
                    {
                        case FileFormatKind.Image when !type.IsDerivedFrom(imageDataSymbol):
                            {
                                // Image file format but not ImageData, report it!
                                var attributes = typesAttributesWithExtension.Select(x => x.Attribute);
                                var diagnostic = Diagnostic.Create(UnexpectedMediaBaseTypeRule,
                                    FindExtensionLocationInAttribute(attributes, typesAttributesWithExtension.Key),
                                    typesAttributesWithExtension.Key,
                                    "inheriting from ImageData");

                                context.ReportDiagnostic(diagnostic);
                                break;
                            }
                        case FileFormatKind.Video when !type.IsDerivedFrom(videoDataSymbol):
                            {
                                // Video file format but not VideoData, report it!
                                var attributes = typesAttributesWithExtension.Select(x => x.Attribute);
                                var diagnostic = Diagnostic.Create(UnexpectedMediaBaseTypeRule,
                                    FindExtensionLocationInAttribute(attributes, typesAttributesWithExtension.Key),
                                    typesAttributesWithExtension.Key,
                                    "inheriting from VideoData");

                                context.ReportDiagnostic(diagnostic);
                                break;
                            }
                        case FileFormatKind.Generic:
                            if (type.IsDerivedFrom(imageDataSymbol))
                            {
                                // Suggest against using ImageData
                                var attributes = typesAttributesWithExtension.Select(x => x.Attribute);
                                var diagnostic = Diagnostic.Create(UnexpectedMediaBaseTypeRule,
                                    FindExtensionLocationInAttribute(attributes, typesAttributesWithExtension.Key),
                                    typesAttributesWithExtension.Key,
                                    "not inheriting from ImageData");

                                context.ReportDiagnostic(diagnostic);
                                break;
                            }
                            else if (type.IsDerivedFrom(videoDataSymbol))
                            {
                                // Suggest against using VideoData
                                var attributes = typesAttributesWithExtension.Select(x => x.Attribute);
                                var diagnostic = Diagnostic.Create(UnexpectedMediaBaseTypeRule,
                                    FindExtensionLocationInAttribute(attributes, typesAttributesWithExtension.Key),
                                    typesAttributesWithExtension.Key,
                                    "not inheriting from VideoData");

                                context.ReportDiagnostic(diagnostic);
                                break;
                            }
                            break;
                        case FileFormatKind.Unknown:
                            // The file extension is not in any of the lists of common file types so we can't tell
                            // if the chosen base content type is wrong or not
                            break;
                    }
                }
            }
        }

        private readonly char[] commaCharArray = new[] { ',' };
        private List<string> GetAllExtensionsFromAttribute(AttributeData attribute, SymbolAnalysisContext context)
        {
            bool anyArgumentPresent = false;
            List<string> extensions = new List<string>();
            if (attribute.TryGetArgument("ExtensionString", out var argument))
            {
                anyArgumentPresent = true;
                var extString = (string)argument.Value;
                foreach (var val in extString.Split(commaCharArray, StringSplitOptions.RemoveEmptyEntries))
                    extensions.Add(CleanExtensionValue(val));
            }
            if (attribute.TryGetArgument("Extensions", out argument))
            {
                anyArgumentPresent = true;
                foreach (var val in argument.Values)
                {
                    extensions.Add(CleanExtensionValue((string)val.Value));
                }
            }

            if (!anyArgumentPresent)
            {
                var diagnostic = Diagnostic.Create(MissingMediaDescriptorArgumentsRule, attribute.GetLocation());
                context.ReportDiagnostic(diagnostic);
                return extensions;
            }

            return extensions.Where(ext => !string.IsNullOrWhiteSpace(ext))
                .Distinct()
                .ToList();
        }

        private string CleanExtensionValue(string rawExtension)
        {
            return rawExtension?.Trim()
                ?.TrimStart('.')
                ?.ToUpperInvariant();
        }

        private Location FindExtensionLocationInAttribute(IEnumerable<AttributeData> attributes, string extension)
        {
            foreach (var attribute in attributes)
            {
                string source = attribute.ApplicationSyntaxReference.GetSyntax().ToFullString().ToUpperInvariant();
                // Test if this MediaDescriptorAttribute has the given extension occurring in it.
                // It must not have another letter just before or after it (e.g. "DOC" must not match "DOCX")
                Match match = Regex.Match(source, $@"\W{extension}\W");
                if (match.Success)
                {
                    Location location = attribute.GetLocation();
                    int from = location.SourceSpan.Start + match.Index + 1;
                    TextSpan span = new TextSpan(from, extension.Length);
                    return Location.Create(location.SourceTree, span);
                }
            }

            throw new Exception($"Expected to find extension \"{extension}\" in one of the given MediaDescriptor attributes");
        }

        private readonly string[] imageFormats = new string[] { "PNG", "JPG", "JPEG", "GIF", "WEBP", "APNG", "AVIF" };
        private readonly string[] videoFormats = new string[] { "MP4", "M4V", "MOV", "WEBM", "MKV", "FLV", "MWV" };
        private readonly string[] genericFormats = new string[] { "ZIP", "PDF", "DOC", "DOCX", "XLS", "XLSX", "PPT", "PPTX" };
        private FileFormatKind GetFileFormatKind(string fileFormat)
        {
            if (imageFormats.Contains(fileFormat))
                return FileFormatKind.Image;
            else if (videoFormats.Contains(fileFormat))
                return FileFormatKind.Video;
            else if (genericFormats.Contains(fileFormat))
                return FileFormatKind.Generic;
            else
                return FileFormatKind.Unknown;
        }

        private enum FileFormatKind
        {
            Image, Video, Generic, Unknown
        }
    }
}
