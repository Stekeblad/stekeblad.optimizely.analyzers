using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.UrlBuilderToStringAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.BadMethods.UrlBuilderToStringCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.BadMethods
{
	[TestClass]
	public class UrlBuilderToStringTest : MyTestClassBase
	{
		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ReplaceUrlBuilderToStringWithCastTest(ReferenceAssemblies assemblies)
		{
			const string test =
				@"using EPiServer;
namespace test
{
    public class C
    {
        public void M()
        {
            var builder = new UrlBuilder(""https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1018.md"");
            var url = {|#0:builder.ToString()|};
        }
    }
}";

			const string fixTest =
				@"using EPiServer;
namespace test
{
    public class C
    {
        public void M()
        {
            var builder = new UrlBuilder(""https://github.com/Stekeblad/stekeblad.optimizely.analyzers/blob/master/doc/Analyzers/SOA1018.md"");
            var url = (string)builder;
        }
    }
}";

			var expected = VerifyCS.Diagnostic(UrlBuilderToStringAnalyzer.DiagnosticID)
				.WithLocation(0);

			await VerifyCS.VerifyCodeFixAsync(test, assemblies, expected, fixTest);
		}
	}
}
