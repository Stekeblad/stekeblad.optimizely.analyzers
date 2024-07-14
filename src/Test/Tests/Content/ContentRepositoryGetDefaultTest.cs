using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.ContentRepositoryGetDefaultAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
    [TestClass]
    public class ContentRepositoryGetDefaultTest
    {
        [TestMethod]
        public async Task NewContentType_Match()
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

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
        }

        [TestMethod]
        public async Task NewRegularType_NoMatch()
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

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }
    }
}
