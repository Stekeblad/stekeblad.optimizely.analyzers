using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.Content.ContentRepositoryAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
	[TestClass]
	public class ContentRepositoryTest : MyTestClassBase
	{
		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_RegisteredNonAbstractContentType_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					[EPiServer.DataAnnotations.ContentType(GUID = ""5e390e2d-79da-4fc1-83cc-8ad1089e44ca"")]
					public class ArticlePage : EPiServer.Core.PageData {}
					public static class ArticleCreator
					{
						public static ArticlePage CreateArticle(EPiServer.Core.ContentReference parent,
							EPiServer.IContentRepository contentRepo)
						{
							return contentRepo.GetDefault<ArticlePage>(parent);
						}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_AbstractContentType_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public abstract class ArticlePage : EPiServer.Core.PageData {}
					public static class ArticleCreator
					{
						public static ArticlePage CreateArticle(EPiServer.Core.ContentReference parent,
							EPiServer.IContentRepository contentRepo)
						{
							return contentRepo.GetDefault<{|#0:ArticlePage|}>(parent);
						}
					}
				}";

			var expected = VerifyCS.Diagnostic(ContentRepositoryAnalyzer.GetDefaultDiagnosticId)
				.WithLocation(0)
				.WithArguments("ArticlePage");
			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_AbstractContentTypeWithContentTypeAttribute_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					[EPiServer.DataAnnotations.ContentType(GUID = ""5e390e2d-79da-4fc1-83cc-8ad1089e44ca"")]
					public abstract class ArticlePage : EPiServer.Core.PageData {}
					public static class ArticleCreator
					{
						public static ArticlePage CreateArticle(EPiServer.Core.ContentReference parent,
							EPiServer.IContentRepository contentRepo)
						{
							return contentRepo.GetDefault<{|#0:ArticlePage|}>(parent);
						}
					}
				}";

			var expected = VerifyCS.Diagnostic(ContentRepositoryAnalyzer.GetDefaultDiagnosticId)
				.WithLocation(0)
				.WithArguments("ArticlePage");
			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_NotAbstractContentTypeWithoutContentTypeAttribute_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData {}
					public static class ArticleCreator
					{
						public static ArticlePage CreateArticle(EPiServer.Core.ContentReference parent,
							EPiServer.IContentRepository contentRepo)
						{
							return contentRepo.GetDefault<{|#0:ArticlePage|}>(parent);
						}
					}
				}";

			var expected = VerifyCS.Diagnostic(ContentRepositoryAnalyzer.GetDefaultDiagnosticId)
				.WithLocation(0)
				.WithArguments("ArticlePage");
			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_TypeIsInterface_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public interface IContentWithHeading : EPiServer.Core.IContentData { string Heading { get; set; } }
					public static class ContentCreator
					{
						public static IContentWithHeading CreateWithHeading(EPiServer.Core.ContentReference parent,
							EPiServer.IContentRepository contentRepo)
						{
							return contentRepo.GetDefault<{|#0:IContentWithHeading|}>(parent);
						}
					}
				}";

			var expected = VerifyCS.Diagnostic(ContentRepositoryAnalyzer.GetDefaultDiagnosticId)
				.WithLocation(0)
				.WithArguments("IContentWithHeading");
			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_ContentAssetsFolder_NoMatch(ReferenceAssemblies assemblies)
		{
			// ContentAssetFolder can be instantiated but it is actually not decorated with ContentTypeAttribute
			const string test = @"
				namespace tests
				{
					public static class ContentCreator
					{
						public static EPiServer.Core.ContentAssetFolder CreateAssetsFolder(EPiServer.Core.ContentReference parent,
							EPiServer.IContentRepository contentRepo)
						{
							return contentRepo.GetDefault<EPiServer.Core.ContentAssetFolder>(parent);
						}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_ContentFolder_NoMatch(ReferenceAssemblies assemblies)
		{
			// ContentFolder can be instantiated but it is actually not decorated with ContentTypeAttribute
			const string test = @"
				namespace tests
				{
					public static class ContentCreator
					{
						public static EPiServer.Core.ContentFolder CreateFolder(EPiServer.Core.ContentReference parent,
							EPiServer.IContentRepository contentRepo)
						{
							return contentRepo.GetDefault<EPiServer.Core.ContentFolder>(parent);
						}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task GetDefault_IgnoreWhenGeneric_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				using EPiServer.Core;
				namespace tests
				{
					public static class ContentCreator
					{
						public static T CreateBelowStart<T>(this EPiServer.IContentRepository contentRepo) where T : PageData
						{
							return contentRepo.GetDefault<T>(ContentReference.StartPage);
						}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}
	}
}
