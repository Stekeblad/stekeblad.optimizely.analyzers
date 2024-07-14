using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.UrlBuilderToStringAnalyzer,
    Stekeblad.Optimizely.Analyzers.CodeFixes.BadMethods.UrlBuilderToStringCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.BadMethods
{
    [TestClass]
    public class UrlBuilderToStringTest
    {
        [TestMethod]
        public async Task ReplaceUrlBuilderToStringWithCastTest()
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

            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_10, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
        }
    }
}
