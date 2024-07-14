using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.Content.FullOptimizelyPropertyAnalyzer,
    Stekeblad.Optimizely.Analyzers.CodeFixes.Content.FullOptimizelyPropertyCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
    [TestClass]
    public class FullOptimizelyPropertyTest
    {
        [TestMethod]
        public async Task NoOptiProperties_NoMatch()
        {
            const string test = @"
				namespace Tests
				{
					public class Test
					{
						private readonly string _field;

						protected Test()
						{
							_field = ""fieldValue"";
						}

						public static Test NewTest()
						{
							return new Test();
						}

						public string Prop { get; set; }
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task OptiPropertyWithLambdaGetterAndSetter_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public string Heading
						{
							get => throw new System.NotImplementedException();
							set => throw new System.NotImplementedException();
						}
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task OptiPropertyWithBodyGetterAndSetter_NoMatch()
        {
            const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public string Heading
						{
							get
							{
								throw new System.NotImplementedException();
							}
							set
							{
								throw new System.NotImplementedException();
							}
						}
					}
				}";

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
        }

        [TestMethod]
        public async Task OptiPropertyWithBodyGetterAndSetterBody_Match()
        {
            const string test = @"
using EPiServer.Core;

namespace tests
{
    public class ArticlePage : EPiServer.Core.PageData
    {
        public string {|#0:Heading|} { get; set; }
    }
}";

            const string fixTest = @"
using EPiServer.Core;

namespace tests
{
    public class ArticlePage : EPiServer.Core.PageData
    {
        public string Heading
        {
            get
            {
                var heading = this.GetPropertyValue(p => p.Heading);
                return heading;
            }

            set
            {
                this.SetPropertyValue(p => p.Heading, value);
            }
        }
    }
}";

            var expected = VerifyCS.Diagnostic(FullOptimizelyPropertyAnalyzer.FullOptimizelyPropertyDiagnosticId)
                .WithLocation(0);

            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
        }
    }
}
