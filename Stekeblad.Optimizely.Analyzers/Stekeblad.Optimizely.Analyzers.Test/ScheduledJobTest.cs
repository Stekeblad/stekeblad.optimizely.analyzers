using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using JobHasNoAttributeVerifier = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs.JobHasNoAttributeAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.ScheduledJobs.JobHasNoAttributeCodeFixProvider>;
using JobHasNoBaseVerifier = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs.JobHasBaseClassAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.ScheduledJobs.JobHasBaseClassCodeFixProvider>;
using JobHasNoGuidVerifier = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs.ScheduledPluginAttributeHasNoGuidAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test
{
	[TestClass]
	public class ScheduledJobTest
	{
		[TestMethod]
		public async Task JobWithCompleteDeclaration_NoMatch()
		{
			const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""I AM A VALID GUID"")]
					public class MyTestScheduledJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished sucessfully"";
					}
				}";

			await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
			await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
			await JobHasNoGuidVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
		}

		[TestMethod]
		public async Task ScheduledPluginAttributeWithoutGuid_MatchOn11Plus()
		{
			const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[ScheduledPlugIn(DisplayName = ""TestJob"")]
					public class {|#0:MyTestScheduledJob|} : ScheduledJobBase
					{
						public override string Execute() => ""Job finished sucessfully"";
					}
				}";

			var expected = JobHasNoGuidVerifier.Diagnostic(ScheduledPluginAttributeHasNoGuidAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("MyTestScheduledJob");

			await JobHasNoGuidVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_10);
			await JobHasNoGuidVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
			await JobHasNoGuidVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
		}

		[TestMethod]
		public async Task JobWithoutAttribute_Match()
		{
			const string test =
@"using EPiServer.Scheduler;
				
namespace tests
{
	public class {|#0:MyTestScheduledJob|} : ScheduledJobBase
	{
		public override string Execute() => ""Job finished sucessfully"";
	}
}";

			const string fixTest =
@"using EPiServer.Scheduler;
using EPiServer.PlugIn;

namespace tests
{
    [ScheduledPlugIn]
    public class {|#0:MyTestScheduledJob|} : ScheduledJobBase
    {
        public override string Execute() => ""Job finished sucessfully"";
    }
}";

			var expected = JobHasNoAttributeVerifier.Diagnostic(JobHasNoAttributeAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("MyTestScheduledJob");

			await JobHasNoAttributeVerifier.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fixTest);
		}

		[TestMethod]
		public async Task JobWithoutBaseClass_Match()
		{
			const string test =
				@"using EPiServer.PlugIn;
				
				namespace tests
				{
					[ScheduledPlugIn]
					public class {|#0:MyTestScheduledJob|}
					{
						public string Execute() => ""Job finished sucessfully"";
					}
				}";

			var expected = JobHasNoBaseVerifier.Diagnostic(JobHasBaseClassAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("MyTestScheduledJob");

			await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
		}
	}
}
