using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using JobHasNoAttributeVerifier = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs.UseScheduledPluginAttributeAnalyzer,
    Stekeblad.Optimizely.Analyzers.CodeFixes.ScheduledJobs.JobHasNoAttributeCodeFixProvider>;
using JobHasNoBaseVerifier = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs.JobHasBaseClassAnalyzer,
    Stekeblad.Optimizely.Analyzers.CodeFixes.ScheduledJobs.JobHasBaseClassCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.ScheduledJobs
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
					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
					public class MyTestScheduledJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}
				}";

            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
            await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

		[TestMethod]
		public async Task JobWithCompleteDeclaration_guidConstant_NoMatch()
		{
			const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = guid)]
					public class MyTestScheduledJob : ScheduledJobBase
					{
						private const string guid = ""01234567-89ab-cdef-0123-456789abcdef"";
						public override string Execute() => ""Job finished successfully"";
					}
				}";

			await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
			await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
		}

		[TestMethod]
        public async Task CompleteJobButImplementingInterface_NoMatch()
        {
            const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler.Internal;
				using System;
				
				namespace tests
				{
					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
					public class MyTestScheduledJob : IScheduledJob
					{
						public Guid ID { get; }
						public string Execute() => ""Job finished successfully"";
					}
				}";

            await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task MultipleValidJobDefinitions_NoMatch()
        {
            const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""11111111-1111-1111-1111-111111111111"")]
					public class MyTestScheduledJob1 : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}

					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""22222222-2222-2222-2222-222222222222"")]
					public class MyTestScheduledJob2 : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}

					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""33333333-3333-3333-3333-333333333333"")]
					public class MyTestScheduledJob3 : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}
				}";

            await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
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
						public override string Execute() => ""Job finished successfully"";
					}
				}";

            var expected = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.MissingAttributeDiagnosticId)
                .WithLocation(0)
                .WithArguments("MyTestScheduledJob");

            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task ScheduledPluginAttributeWithoutGuid_Match()
        {
            const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[{|#0:ScheduledPlugIn(DisplayName = ""TestJob"")|}]
					public class MyTestScheduledJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}
				}";

            var expected = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.MissingGuidDiagnosticId)
                .WithLocation(0)
                .WithArguments("MyTestScheduledJob");

            // PackageCollections.Core_10 uses the oldest available 10.x release, the GUID attribute was added in 10.3
            // and that test should therefore not report an error
            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_10);
            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected);
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
						public string Execute() => ""Job finished successfully"";
					}
				}";

            var expected = JobHasNoBaseVerifier.Diagnostic(JobHasBaseClassAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("MyTestScheduledJob");

            await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task ScheduledPluginAttributeWithEmptyGuid_Match()
        {
            const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[{|#0:ScheduledPlugIn(DisplayName = ""TestJob"", GUID = """")|}]
					public class MyTestScheduledJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}
				}";

            var expected = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.InvalidGuidDiagnosticId)
                .WithLocation(0)
                .WithArguments("MyTestScheduledJob");

            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task ScheduledPluginAttributeWithInvalidGuid_Match()
        {
            const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[{|#0:ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""I AM A VALID GUID"")|}]
					public class MyTestScheduledJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}
				}";

            var expected = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.InvalidGuidDiagnosticId)
                .WithLocation(0)
                .WithArguments("MyTestScheduledJob");

            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task ScheduledPluginAttributeWithReusedGuid_Match()
        {
            const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[{|#0:ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")|}]
					public class MyTestScheduledJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}

					[{|#1:ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")|}]
					public class MySecondJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}
				}";

            var expected0 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.GuidReusedDiagnosticId)
                .WithLocation(0)
                .WithArguments("MyTestScheduledJob", "MySecondJob, MyTestScheduledJob");

            var expected1 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.GuidReusedDiagnosticId)
                .WithLocation(1)
                .WithArguments("MySecondJob", "MySecondJob, MyTestScheduledJob");

            await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, new[] { expected0, expected1 });
        }

		[TestMethod]
		public async Task AbstractJobWithoutAttribute_NoMatch()
		{
			const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					public abstract class MyTestScheduledJob : ScheduledJobBase
					{
					}
				}";

			await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
			await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
		}

		[TestMethod]
		public async Task AbstractJobWithAttribute_Match()
		{
			const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[ScheduledPlugIn(DisplayName = ""TestJob"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")]
					public {|#0:abstract|} class MyTestScheduledJob : ScheduledJobBase
					{
					}
				}";

			var expected0 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.AttributeOnAbstractDiagnosticId)
				.WithLocation(0)
				.WithArguments("MyTestScheduledJob");

			await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0);
			await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
		}

		[TestMethod]
		public async Task AbstractJobSharingGuidWithConcreateJob_Match()
		{
			const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
#pragma warning disable SOA1039 // Remove attribute from abstract class
					[{|#0:ScheduledPlugIn(DisplayName = ""AbstractTestJob"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")|}]
					public abstract class MyAbstractTestScheduledJob : ScheduledJobBase
					{
					}
#pragma warning restore SOA1039

					[{|#1:ScheduledPlugIn(DisplayName = ""RealTestJob"", GUID = ""01234567-89ab-cdef-0123-456789abcdef"")|}]
					public class MyRealTestScheduledJob : ScheduledJobBase
					{
						public override string Execute() => ""Job finished successfully"";
					}
				}";

			var expected0 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.GuidReusedDiagnosticId)
				.WithLocation(0)
				.WithArguments("MyAbstractTestScheduledJob", "MyAbstractTestScheduledJob, MyRealTestScheduledJob");

			var expected1 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.GuidReusedDiagnosticId)
				.WithLocation(1)
				.WithArguments("MyRealTestScheduledJob", "MyAbstractTestScheduledJob, MyRealTestScheduledJob");

			await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0, expected1);
			await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
		}

		[TestMethod]
		public async Task JobWithInvalidName_Match()
		{
			const string test = @"
				using EPiServer.PlugIn;
				using EPiServer.Scheduler;
				
				namespace tests
				{
					[{|#0:ScheduledPlugIn(GUID = ""00000000-1111-0000-0000-000000000000"")|}]
					public class NoNameAtAll : ScheduledJobBase
					{ public override string Execute() => ""Job finished successfully""; }

					[{|#1:ScheduledPlugIn(DisplayName = null, GUID = ""00000000-2222-0000-0000-000000000000"")|}]
					public class NullName : ScheduledJobBase
					{ public override string Execute() => ""Job finished successfully""; }

					[{|#2:ScheduledPlugIn(DisplayName = """", GUID = ""00000000-3333-0000-0000-000000000000"")|}]
					public class EmptyName : ScheduledJobBase
					{ public override string Execute() => ""Job finished successfully""; }

					[{|#3:ScheduledPlugIn(LanguagePath = """", GUID = ""00000000-4444-0000-0000-000000000000"")|}]
					public class NullLangPath : ScheduledJobBase
					{ public override string Execute() => ""Job finished successfully""; }



					[ScheduledPlugIn(DisplayName = ""NameNoLang"", LanguagePath = """", GUID = ""00000000-5555-0000-0000-000000000000"")]
					public class NameNoLang : ScheduledJobBase
					{ public override string Execute() => ""Job finished successfully""; }

					[ScheduledPlugIn(DisplayName = """", LanguagePath = ""LangNoName"", GUID = ""00000000-6666-0000-0000-000000000000"")]
					public class LangNoName : ScheduledJobBase
					{ public override string Execute() => ""Job finished successfully""; }
				}";

			var expected0 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.MissingNameDiagnosticId)
				.WithLocation(0)
				.WithArguments("NoNameAtAll");

			var expected1 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.MissingNameDiagnosticId)
				.WithLocation(1)
				.WithArguments("NullName");

			var expected2 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.MissingNameDiagnosticId)
				.WithLocation(2)
				.WithArguments("EmptyName");

			var expected3 = JobHasNoAttributeVerifier.Diagnostic(UseScheduledPluginAttributeAnalyzer.MissingNameDiagnosticId)
				.WithLocation(3)
				.WithArguments("NullLangPath");

			await JobHasNoAttributeVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11,
				expected0, expected1, expected2, expected3);
			await JobHasNoBaseVerifier.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
		}
	}
}
