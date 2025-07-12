using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.Content.ContentTypeConstructorAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.Content.ContentTypeConstructorCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
	[TestClass]
	public class ContentTypeConstructorTest
	{
		[TestMethod]
		public async Task EmptyConstructor_NoMatch()
		{
			const string test =
				@"using EPiServer.Core;
namespace test
{
	public class TestBlock : BlockData
    {
        public TestBlock()
        {
        }
    }
}";

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
		}

		[TestMethod]
		public async Task ConstructorWithDI_Match()
		{
			const string test =
				@"using EPiServer.Core;
using EPiServer;
namespace test
{
	public class TestBlock : BlockData
    {
        private readonly IContentLoader _contentLoader;
        public TestBlock {|#0:(IContentLoader contentLoader)|}
        {
            _contentLoader = contentLoader;
        }
    }
}";

			var expected = VerifyCS.Diagnostic(ContentTypeConstructorAnalyzer.ConstructorParametersDiagnosticID)
				.WithLocation(0);

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
		}

		[TestMethod]
		public async Task ConstructorSettingDefaultValues_Match()
		{
			const string test =
				@"using EPiServer.Core;
using EPiServer;
namespace test
{
	public class TestBlock : BlockData
    {
        private readonly string roField;
        private bool MethodCall() => true;
        public TestBlock()
        {
            {|#0:Heading = ""Default Heading""|};
            roField = ""ro string"";
            string stringVar;
            stringVar = ""string"";
            var stringVar2 = ""str"" + ""ing"";
            MethodCall();
        }
        public virtual string Heading { get; set; }
    }
}";

			var expected = VerifyCS.Diagnostic(ContentTypeConstructorAnalyzer.ConstructorInitializationDiagnosticID)
				.WithLocation(0);

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
		}

		[TestMethod]
		public async Task ConstructorSettingDefaultValues_FixTest()
		{
			const string test =
				@"using EPiServer.Core;
using EPiServer;

namespace test
{
	public class TestBlock : BlockData
    {
        private readonly string roField;
        private bool MethodCall() => true;
        public TestBlock()
        {
            {|#0:Heading = ""Default Heading""|};
            roField = ""ro string"";
            string stringVar;
            stringVar = ""string"";
            var stringVar2 = ""str"" + ""ing"";
            MethodCall();
        }
        public virtual string Heading { get; set; }
    }
}";

			const string fixTest =
				@"using EPiServer.Core;
using EPiServer;
using EPiServer.DataAbstraction;

namespace test
{
	public class TestBlock : BlockData
    {
        private readonly string roField;
        private bool MethodCall() => true;
        public TestBlock()
        {
            roField = ""ro string"";
            string stringVar;
            stringVar = ""string"";
            var stringVar2 = ""str"" + ""ing"";
            MethodCall();
        }
        public virtual string Heading { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Heading = ""Default Heading"";
        }
    }
}";

			var expected = VerifyCS.Diagnostic(ContentTypeConstructorAnalyzer.ConstructorInitializationDiagnosticID)
				.WithLocation(0);

			await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_10, expected, fixTest);
			await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
			await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
		}
	}
}
