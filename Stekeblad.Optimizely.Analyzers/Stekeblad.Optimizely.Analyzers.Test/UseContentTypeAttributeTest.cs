using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.Content.UseContentTypeAttributeAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.UseContentTypeAttributeCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test
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
		public async Task PageTypeWithoutAttribute_Match()
		{
			const string test =
				@"namespace tests
				{
					public class {|#0:ArticlePage|} : EPiServer.Core.PageData {}
				}";

			var expected = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.DiagnosticId)
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
					[EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")]
					public class {|#0:ArticlePage|} : EPiServer.Core.PageData {}
				}";

			var expected = VerifyCS.Diagnostic(UseContentTypeAttributeAnalyzer.SecondRule)
				.WithLocation(0)
				.WithArguments("ContentTypeAttribute", "ArticlePage");
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
		}
	}
}
