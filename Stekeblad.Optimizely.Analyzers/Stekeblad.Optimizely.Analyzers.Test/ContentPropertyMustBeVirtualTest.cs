using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.ContentPropertyMustBeVirtualAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.ContentPropertyMustBeVirtualCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test
{
	[TestClass]
	public class ContentPropertyMustBeVirtualTest
	{
		[TestMethod]
		public async Task PageTypeWithNoPropertties_NoMatch()
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData {}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test);
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

			await VerifyCS.VerifyAnalyzerAsync(test);
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
			await VerifyCS.VerifyCodeFixAsync(test, expected, fixTest);
		}

		[TestMethod]
		public async Task PageTypeWithOverridenVirtualProperty_NoMatch()
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

			await VerifyCS.VerifyAnalyzerAsync(test);
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

			await VerifyCS.VerifyAnalyzerAsync(test);
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

			await VerifyCS.VerifyAnalyzerAsync(test);
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
			await VerifyCS.VerifyCodeFixAsync(test, expected, fixTest);
		}

		[TestMethod]
		public async Task ExcludePopertyWithoutSetter_NoMatch()
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public string Heading => ""Random page title"";
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test);
		}
	}
}
