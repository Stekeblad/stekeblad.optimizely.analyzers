using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;

using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.Content.UseSetDefaultValuesAnalyzer,
    Stekeblad.Optimizely.Analyzers.CodeFixes.Content.UseSetDefaultValuesCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
    [TestClass]
    public class UseSetDefaultValuesTest
    {
        [TestMethod]
        public async Task DefaultValuesTest_DiagnoseOnly()
        {
            const string test = @"using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
	[ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
	public class DefaultValuesPage : PageData
	{
		public virtual string A { get; set; }
		public virtual string B { get; set; } {|#0:= ""Default value of B""|};
		private string C { get; set; }
		private string D { get; set; } = ""D for Default value"";
		public string E;
		public string F = ""F for field default value"";

		public override void SetDefaultValues(ContentType contentType)
		{
			base.SetDefaultValues(contentType);

			A = ""a"";
			B = ""b"";
		}
	}
}";

            var expected = VerifyCS.Diagnostic(UseSetDefaultValuesAnalyzer.DiagnosticID)
                .WithLocation(0)
                .WithArguments("B");

            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_10, expected);
            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
            await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
        }

        [TestMethod]
        public async Task CreateMissingSetDefaultValuesMethodWhenFixing()
        {
            const string test = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesPage : PageData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; } {|#0:= ""Default value of B""|};
    }
}";

            const string fixTest = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesPage : PageData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            B = ""Default value of B"";
        }
    }
}";

            var expected = VerifyCS.Diagnostic(UseSetDefaultValuesAnalyzer.DiagnosticID)
                .WithLocation(0)
                .WithArguments("B");

            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_10, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
        }

        [TestMethod]
        public async Task AddAssignmentIfNotPresentWhenFixing()
        {
            const string test = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesPage : PageData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; } {|#0:= ""Default value of B""|};

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
        }
    }
}";

            const string fixTest = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesPage : PageData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; }
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            B = ""Default value of B"";
        }
    }
}";

            var expected = VerifyCS.Diagnostic(UseSetDefaultValuesAnalyzer.DiagnosticID)
                .WithLocation(0)
                .WithArguments("B");

            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_10, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
        }

        [TestMethod]
        public async Task UpdateExistingAssignmentIfPresentWhenFixing()
        {
            const string test = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesPage : PageData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; } {|#0:= ""Default value of B""|};

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            B = ""This value will be replaced"";
        }
    }
}";

            const string fixTest = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesPage : PageData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; }
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            B = ""Default value of B"";
        }
    }
}";

            var expected = VerifyCS.Diagnostic(UseSetDefaultValuesAnalyzer.DiagnosticID)
                .WithLocation(0)
                .WithArguments("B");

            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_10, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
        }

        [TestMethod]
        public async Task CanDiagnoseAndFixBlockData()
        {
            const string test = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesBlock : BlockData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; } {|#0:= ""Default value of B""|};
    }
}";

            const string fixTest = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesBlock : BlockData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            B = ""Default value of B"";
        }
    }
}";

            var expected = VerifyCS.Diagnostic(UseSetDefaultValuesAnalyzer.DiagnosticID)
                .WithLocation(0)
                .WithArguments("B");

            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_10, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
        }

        [TestMethod]
        public async Task CanDiagnoseAndFixMediaData()
        {
            const string test = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesFile : MediaData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; } {|#0:= ""Default value of B""|};
    }
}";

            const string fixTest = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;

namespace tests
{
    [ContentType(GroupName = ""Content"", GUID = ""7c74f40d-cac8-4584-b5b4-09fc3e55e2b2"")]
    public class DefaultValuesFile : MediaData
    {
        public virtual string A { get; set; }
        public virtual string B { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            B = ""Default value of B"";
        }
    }
}";

            var expected = VerifyCS.Diagnostic(UseSetDefaultValuesAnalyzer.DiagnosticID)
                .WithLocation(0)
                .WithArguments("B");

            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_10, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
            await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fixTest);
        }
    }
}
