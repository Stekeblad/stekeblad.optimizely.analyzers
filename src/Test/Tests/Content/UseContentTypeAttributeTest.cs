using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.Content.UseContentTypeAttributeAnalyzer,
    Stekeblad.Optimizely.Analyzers.CodeFixes.Content.UseContentTypeAttributeCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
    [TestClass]
    public class UseContentTypeAttributeTest
    {
        [TestMethod]
        public async Task EmptyFile_NoMatch()
        {
            const string test = "";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task PageTypeWithFullyDefinedAttribute_NoMatch()
        {
            const string test = @"
				using EPiServer.DataAnnotations;

				namespace tests
				{
					[ContentType(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
					public class ArticlePage : EPiServer.Core.PageData {}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task AbstractPageTypeWithoutAttribute_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public abstract class ArticlePage : EPiServer.Core.PageData {}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task MultipleValidContentDefinitions_NoMatch()
        {
            const string test = @"
				using EPiServer.DataAnnotations;

				namespace tests
				{
					[ContentType(GroupName = ""Content"", GUID = ""11111111-1111-1111-1111-111111111111"")]
					public class ArticlePage1 : EPiServer.Core.PageData {}

					[ContentType(GroupName = ""Content"", GUID = ""22222222-2222-2222-2222-222222222222"")]
					public class ArticlePage2 : EPiServer.Core.PageData {}

					[ContentType(GroupName = ""Content"", GUID = ""33333333-3333-3333-3333-333333333333"")]
					public class ArticlePage3 : EPiServer.Core.PageData {}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task PageTypeWithoutAttribute_Match()
        {
            const string test =
                @"namespace tests
				{
					public class {|#0:ArticlePage|} : EPiServer.Core.PageData {}
				}";

            var expected = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.MissingAttributeDiagnosticId)
                .WithLocation(0)
                .WithArguments("ArticlePage");
            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task PageTypeWithAttributeButNoGuid_Match()
        {
            const string test = @"
				namespace tests
				{
					[{|#0:EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")|}]
					public class ArticlePage : EPiServer.Core.PageData {}
				}";

            var expected = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.MissingGuidDiagnosticId)
                .WithLocation(0)
                .WithArguments("ContentTypeAttribute", "ArticlePage");
            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task PageTypeWithAttributeAndEmptyGuid_Match()
        {
            const string test = @"
				namespace tests
				{
					[{|#0:EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"", GUID = """")|}]
					public class ArticlePage : EPiServer.Core.PageData {}
				}";

            var expected = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.InvalidGuidDiagnosticId)
                .WithLocation(0)
                .WithArguments("ArticlePage");
            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task PageTypeWithAttributeAndInvalidGuid_Match()
        {
            const string test = @"
				namespace tests
				{
					[{|#0:EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"", GUID = ""I AM A VALID GUID"")|}]
					public class ArticlePage : EPiServer.Core.PageData {}
				}";

            var expected = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.InvalidGuidDiagnosticId)
                .WithLocation(0)
                .WithArguments("ArticlePage");
            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task PageTypeWithAttributeAndReusedGuid_Match()
        {
            const string test = @"
				namespace tests
				{
					[{|#0:EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")|}]
					public class ArticlePage : EPiServer.Core.PageData {}

					[{|#1:EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")|}]
					public class ArticlePage2 : EPiServer.Core.PageData {}

                  [{|#2:EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")|}]
					public class ArticlePage3 : EPiServer.Core.PageData {}

                  [EPiServer.DataAnnotations.ContentTypeAttribute(     GroupName = ""Content"", GUID = ""01234567-89ab-cdef-fedc-ba9876543210"")  ]
                  public class NewsPage : EPiServer.Core.PageData {}
				}";

            var expected0 = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.GuidReusedDiagnosticId)
                .WithLocation(0)
                .WithArguments("ArticlePage, ArticlePage2, ArticlePage3");
            var expected1 = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.GuidReusedDiagnosticId)
                .WithLocation(1)
                .WithArguments("ArticlePage, ArticlePage2, ArticlePage3");
            var expected2 = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.GuidReusedDiagnosticId)
                .WithLocation(2)
                .WithArguments("ArticlePage, ArticlePage2, ArticlePage3");
            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, new[] { expected0, expected1, expected2 });
        }
    }
}
