using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.ContentRepositoryGetDefaultAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.BadMethods
{
	[TestClass]
	public class ContentRepositoryGetDefaultTest : MyTestClassBase
	{
		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task NewContentType_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData {}

					public static class ArticleCreator
					{
						public static ArticlePage CreateArticle()
						{
							return {|#0:new ArticlePage()|};
						}
					}
				}";

			var expected = VerifyCS.Diagnostic(ContentRepositoryGetDefaultAnalyzer.UseGetDefaultDiagnosticId)
				.WithLocation(0)
				.WithArguments("ArticlePage");

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task NewRegularType_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticleCreator
					{
						public static ArticleCreator GetCreator()
						{
							return new ArticleCreator();
						}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}
	}
}
