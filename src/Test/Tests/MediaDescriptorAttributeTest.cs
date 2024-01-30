using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.Content.MediaDescriptorAttributeAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests
{
    [TestClass]
    public class MediaDescriptorAttributeTest
    {
        [TestMethod]
        public async Task NormalPageTypeDeclaration_NoMatch()
        {
            const string test = @"
                using EPiServer.DataAnnotations;

                namespace tests
                {
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class ArticlePage : EPiServer.Core.PageData {}
                }";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task PageTypeDeclarationWithMediaDescriptor_Match()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [{|#0:MediaDescriptor(ExtensionString = ""filextension"")|}]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class ArticlePage : EPiServer.Core.PageData {}
                }";

            var expected = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.MediaDescriptorNotExpectedDiagnosticId)
                .WithLocation(0);

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
        }

        [TestMethod]
        public async Task NormalTypeDeclarationWithMediaDescriptor_Match()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [{|#0:MediaDescriptor(ExtensionString = ""filextension"")|}]
                    public class FileMetaModel {}
                }";

            var expected = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.MediaDescriptorNotExpectedDiagnosticId)
                .WithLocation(0);

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
        }

        [TestMethod]
        public async Task GoodMediaTypeWithUnknownExtensions_NoMatch()
        {
            const string test = @"
            using EPiServer.DataAnnotations;
            using EPiServer.Framework.DataAnnotations;

            namespace tests
            {
                [MediaDescriptor(ExtensionString = ""one,two,three"")]
                [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                public class OneTwoThreeFile : EPiServer.Core.MediaData {}
           }";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task MissingMediaDescriptorAttribute_Match()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class {|#0:GenericFile|} : EPiServer.Core.MediaData {}
                }";

            var expected = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UseMediaDescriptorDiagnosticId)
                .WithLocation(0)
                .WithArguments("GenericFile");

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
        }

        [TestMethod]
        public async Task GoodFileTypeCombos_NoMatch()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [MediaDescriptor(ExtensionString = ""pdf"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class PdfFile : EPiServer.Core.MediaData {}

                    [MediaDescriptor(ExtensionString = ""png"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class PngFile : EPiServer.Core.ImageData {}

                    [MediaDescriptor(ExtensionString = ""mp4"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class Mp4File : EPiServer.Core.VideoData {}
                }";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task BadFileTypeCombos_Match()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [MediaDescriptor(ExtensionString = ""pdf,{|#0:jpeg|},{|#1:mov|}"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class GenericFile : EPiServer.Core.MediaData {}

                    [MediaDescriptor(ExtensionString = ""{|#2:doc|},png,{|#3:mkv|}"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class ImageFile : EPiServer.Core.ImageData {}

                    [MediaDescriptor(ExtensionString = ""{|#4:zip|},{|#5:gif|},mp4"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class VideoFile : EPiServer.Core.VideoData {}
                }";

            var expected0 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UnexpectedMediaBaseTypeDiagnosticId)
                .WithLocation(0)
                .WithArguments("JPEG", "inheriting from ImageData");

            var expected1 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UnexpectedMediaBaseTypeDiagnosticId)
                .WithLocation(1)
                .WithArguments("MOV", "inheriting from VideoData");

            var expected2 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UnexpectedMediaBaseTypeDiagnosticId)
                .WithLocation(2)
                .WithArguments("DOC", "not inheriting from ImageData");

            var expected3 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UnexpectedMediaBaseTypeDiagnosticId)
                .WithLocation(3)
                .WithArguments("MKV", "inheriting from VideoData");

            var expected4 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UnexpectedMediaBaseTypeDiagnosticId)
                .WithLocation(4)
                .WithArguments("ZIP", "not inheriting from VideoData");

            var expected5 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UnexpectedMediaBaseTypeDiagnosticId)
                .WithLocation(5)
                .WithArguments("GIF", "inheriting from ImageData");

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12,
                expected0, expected1, expected2, expected3, expected4, expected5);
        }

        [TestMethod]
        public async Task ExtensionReusedOnSameType_NoMatch()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [MediaDescriptor(ExtensionString = ""pdf,zip,pdf"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class ReusedInOneAttributeFile : EPiServer.Core.MediaData {}

                    [MediaDescriptor(ExtensionString = ""png,jpeg"")]
                    [MediaDescriptor(ExtensionString = ""gif,png"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class ReusedOverMultipleAttributesFile : EPiServer.Core.ImageData {}
                }";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task ExtensionReusedOnMultipleTypes_Match()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [MediaDescriptor(ExtensionString = ""pdf,zip,{|#0:docx|}"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class GenericFile : EPiServer.Core.MediaData {}

                    [MediaDescriptor(ExtensionString = ""doc,{|#1:docx|},xls,xlsx,ppt,pptx"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class OfficeFile : EPiServer.Core.MediaData {}
                }";

            var expected0 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.ExtensionReusedDiagnosticId)
                .WithLocation(0)
                .WithArguments("DOCX", "GenericFile, OfficeFile");

            var expected1 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.ExtensionReusedDiagnosticId)
                .WithLocation(1)
                .WithArguments("DOCX", "GenericFile, OfficeFile");

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0, expected1);
        }

        [TestMethod]
        public async Task AbstractMediaTypeWithoutAttribute_NoMatch()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public abstract class GenericFile : EPiServer.Core.MediaData {}
                }";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task DontForgetExtensionsStringArrayParameter_Match()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [MediaDescriptor(ExtensionString = ""{|#0:abc|}"")]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class AbcFile : EPiServer.Core.MediaData {}

                    [MediaDescriptor(Extensions = new string[] { ""png"" } )]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class PngFile : EPiServer.Core.ImageData {}

                    [MediaDescriptor(Extensions = new string[] { ""{|#1:abc|}"", ""mov"", ""mp4"", ""{|#2:jpg|}"" })]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class VideoFile : EPiServer.Core.VideoData {}
                }";

            var expected0 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.ExtensionReusedDiagnosticId)
                .WithLocation(0)
                .WithArguments("ABC", "AbcFile, VideoFile");

            var expected1 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.ExtensionReusedDiagnosticId)
                .WithLocation(1)
                .WithArguments("ABC", "AbcFile, VideoFile");

            var expected2 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.UnexpectedMediaBaseTypeDiagnosticId)
                .WithLocation(2)
                .WithArguments("JPG", "inheriting from ImageData");

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12,
                expected0, expected1, expected2);
        }

        [TestMethod]
        public async Task NoParametersOnMediaDescriptorAttribute_Match()
        {
            const string test = @"
                using EPiServer.DataAnnotations;
                using EPiServer.Framework.DataAnnotations;

                namespace tests
                {
                    [{|#0:MediaDescriptor()|}]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class AFile : EPiServer.Core.MediaData {}

                    [{|#1:MediaDescriptor|}]
                    [ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
                    public class BFile : EPiServer.Core.MediaData {}
                }";

            var expected0 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.MissingMediaDescriptorArgumentsDiagnosticId)
                .WithLocation(0);

            var expected1 = VerifyCS.Diagnostic(MediaDescriptorAttributeAnalyzer.MissingMediaDescriptorArgumentsDiagnosticId)
                .WithLocation(1);

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0, expected1);
        }
    }
}
