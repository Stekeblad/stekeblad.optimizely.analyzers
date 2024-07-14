using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.Content.ContentPropertyMustBeVirtualAnalyzer,
    Stekeblad.Optimizely.Analyzers.CodeFixes.Content.ContentPropertyMustBeVirtualCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
    [TestClass]
    public class ContentPropertyMustBeVirtualTest
    {
        [TestMethod]
        public async Task PageTypeWithNoProperties_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData {}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task PageTypeWithCorrectProperty_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task PageTypeWithPropertyMissingVirtual_Match()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public string {|#0:Heading|} { get; set; }
					}
				}";

            const string fixTest = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
				}";

            var expected = VerifyCS.Diagnostic(ContentPropertyMustBeVirtualAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("Heading");
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
        }

        [TestMethod]
        public async Task PageTypeWithOverriddenVirtualProperty_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class MyBasePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
					public class ArticlePage : MyBasePage
					{
						public override string Heading { get; set; }
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task PrivateProperty_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						private string Heading { get; set; }
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task StaticProperty_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public static string Heading { get; set; }
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task AbstractPageTypesIsNotExcluded_Match()
        {
            const string test = @"
				namespace tests
				{
					public abstract class ArticlePage : EPiServer.Core.PageData
					{
						public string {|#0:Heading|} { get; set; }
					}
				}";

            const string fixTest = @"
				namespace tests
				{
					public abstract class ArticlePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
				}";

            var expected = VerifyCS.Diagnostic(ContentPropertyMustBeVirtualAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("Heading");
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
        }

        [TestMethod]
        public async Task ExcludePropertyWithoutSetter_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public string Heading => ""Random page title"";
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task ExcludePropertyWithIgnoreAttribute_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						[EPiServer.DataAnnotations.Ignore]
						public /*virtual*/ string Heading { get; set; }
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }
    }
}
