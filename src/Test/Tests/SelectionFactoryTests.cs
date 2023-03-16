using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.Content.SelectionFactoryAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests
{
	[TestClass]
	public class SelectionFactoryTests
	{
		/*    Code snippets for reuse    */

		private const string GoodISelectionFactoryImplementation = @"
					public class SelectionFactory : ISelectionFactory
					{
						public System.Collections.Generic.IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
						{
							return new System.Collections.Generic.List<ISelectItem>
							{
								new SelectItem { Text = ""1"", Value = 1 },
								new SelectItem { Text = ""22"", Value = 22 },
								new SelectItem { Text = ""333"", Value = 333 }
							};
						}
					}";

		private const string GoodISelectionQueryImplementation = @"
					public class SelectionQuery : ISelectionQuery
					{
						SelectItem[] _items;
						public SelectionQuery()
						{
							_items = new SelectItem[]
							{ 
								new SelectItem { Text = ""1"", Value = 1 },
								new SelectItem { Text = ""22"", Value = 22 },
								new SelectItem { Text = ""333"", Value = 333 }
							};
						}

						public System.Collections.Generic.IEnumerable<ISelectItem> GetItems(string query)
						{
							throw new System.NotImplementedException();
						}

						public ISelectItem GetItemByValue(string value)
						{
							throw new System.NotImplementedException();
						}
					}";

		private const string GoodEnumSelectionFactoryImplementation = @"
					public enum ColorEnum
					{
						Red = 1, Green = 2, Blue = 3
					}

					public class EnumFactory : EnumSelectionFactory
					{
						public EnumFactory(LocalizationService localizationService) : base(localizationService) { }

						public EnumFactory() : this(LocalizationService.Current) { }

						protected override Type EnumType => typeof(ColorEnum);

						protected override string GetStringForEnumValue(int value)
						{
							switch (value)
							{
								case (int)ColorEnum.Red: return ""Red"";
								case (int)ColorEnum.Green: return ""Green"";
								case (int)ColorEnum.Blue: return ""Blue"";
								default: return null;
							}
						}
					}";

		/// <summary>
		/// <c>{|#0:text|}</c> where 0 is the value of locationNr and text is the value of text
		/// </summary>
		private string Highlight(int locationNr, string text)
		{
			return $"{{|#{locationNr}:{text}|}}";
		}

		/*    Tests starts here    */

		[TestMethod]
		public async Task NoSelectionAttribute_NoMatch()
		{
			const string test = @"
				namespace tests
				{
					[EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")]
					public class TestPage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
		}

		[TestMethod]
		public async Task MultipleValidSelectionAttributes_Match()
		{
			string test = $@"
				using EPiServer.Shell.ObjectEditing;

				namespace tests
				{{
					{GoodISelectionFactoryImplementation}

					[EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")]
					public class TestPage : EPiServer.Core.PageData
					{{
						[SelectOne(SelectionFactoryType = typeof(SelectionFactory))]
						[SelectMany(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual string {Highlight(0, "Heading")} {{ get; set; }}
					}}
				}}";

			var expected = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.MultipleAttributesDiagnosticId)
				.WithLocation(0)
				.WithArguments("Heading");

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
		}

		[TestMethod]
		public async Task SelectionPropertyTypeTests()
		{
			string test = $@"
				using EPiServer.Shell.ObjectEditing;
				using EPiServer.Framework.Localization;
				using EPiServer.Cms.Shell.UI.ObjectEditing.EditorDescriptors.SelectionFactories;
				using System;

				namespace tests
				{{
					{GoodISelectionFactoryImplementation}

					{GoodISelectionQueryImplementation}

					{GoodEnumSelectionFactoryImplementation}

					public class MyBlock : EPiServer.Core.BlockData {{ }}

					[EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")]
					public class TestPage : EPiServer.Core.PageData
					{{
						[SelectOne(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual string OneStr {{ get; set; }}

						[SelectOne(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual int OneInt {{ get; set; }}

						[SelectOne(SelectionFactoryType = typeof(EnumFactory))]
						public virtual ColorEnum OneEnum {{ get; set; }}

						[SelectOne(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual double {Highlight(0, "OneDouble")} {{ get; set; }}

						[SelectOne(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual MyBlock {Highlight(1, "OneBlock")} {{ get; set; }}

						[SelectMany(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual string ManyStr {{ get; set; }}

						[SelectMany(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual int ManyInt {{ get; set; }}

						[SelectMany(SelectionFactoryType = typeof(EnumFactory))]
						public virtual ColorEnum ManyEnum {{ get; set; }}

						[SelectMany(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual double {Highlight(2, "ManyDouble")} {{ get; set; }}

						[SelectMany(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual MyBlock {Highlight(3, "ManyBlock")} {{ get; set; }}

						[AutoSuggestSelection(typeof(SelectionQuery))]
						public virtual string QueryStr {{ get; set; }}

						[AutoSuggestSelection(typeof(SelectionQuery))]
						public virtual int QueryInt {{ get; set; }}

						[AutoSuggestSelection(typeof(SelectionQuery))]
						public virtual double {Highlight(4, "QueryDouble")} {{ get; set; }}

						[AutoSuggestSelection(typeof(SelectionQuery))]
						public virtual MyBlock {Highlight(5, "QueryBlock")} {{ get; set; }}
					}}
				}}";

			var expected0 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.UnsupportedPropTypeDiagnosticId)
				.WithLocation(0)
				.WithArguments("OneDouble", "Double", "SelectOneAttribute");

			var expected1 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.UnsupportedPropTypeDiagnosticId)
				.WithLocation(1)
				.WithArguments("OneBlock", "MyBlock", "SelectOneAttribute");

			var expected2 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.UnsupportedPropTypeDiagnosticId)
				.WithLocation(2)
				.WithArguments("ManyDouble", "Double", "SelectManyAttribute");

			var expected3 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.UnsupportedPropTypeDiagnosticId)
				.WithLocation(3)
				.WithArguments("ManyBlock", "MyBlock", "SelectManyAttribute");

			var expected4 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.UnsupportedPropTypeDiagnosticId)
				.WithLocation(4)
				.WithArguments("QueryDouble", "Double", "AutoSuggestSelectionAttribute");

			var expected5 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.UnsupportedPropTypeDiagnosticId)
				.WithLocation(5)
				.WithArguments("QueryBlock", "MyBlock", "AutoSuggestSelectionAttribute");

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11,
				expected0, expected1, expected2, expected3, expected4, expected5);
		}

		[TestMethod]
		public async Task SelectionFactoryTypeParameterTest()
		{
			string test = $@"
				using EPiServer.Shell.ObjectEditing;

				namespace tests
				{{
					{GoodISelectionFactoryImplementation}

					public class Clazz {{ }} // Bad selection factory implementation

					public abstract class AbstractFactory : SelectionFactory {{ }}

					[EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")]
					public class TestPage : EPiServer.Core.PageData
					{{
						[SelectOne(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual string OneGood {{ get; set; }}

						[{Highlight(0, "SelectOne(SelectionFactoryType = typeof(Clazz))")}]
						public virtual string OneBad {{ get; set; }}

						[{Highlight(1, "SelectOne(SelectionFactoryType = typeof(AbstractFactory))")}]
						public virtual string OneAbstract {{ get; set; }}

						[{Highlight(2, "SelectOne")}]
						public virtual string OneMissing {{ get; set; }}

						[SelectMany(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual string ManyGood {{ get; set; }}

						[{Highlight(3, "SelectMany(SelectionFactoryType = typeof(Clazz))")}]
						public virtual string ManyBad {{ get; set; }}

						[{Highlight(4, "SelectMany(SelectionFactoryType = typeof(AbstractFactory))")}]
						public virtual string ManyAbstract {{ get; set; }}

						[{Highlight(5, "SelectMany")}]
						public virtual string ManyMissing {{ get; set; }}
					}}
				}}";

			var expected0 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidFactoryTypeDiagnosticId)
				.WithLocation(0)
				.WithArguments("Clazz");

			var expected1 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidFactoryTypeDiagnosticId)
				.WithLocation(1)
				.WithArguments("AbstractFactory");

			var expected2 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.MissingFactoryTypeParamDiagnosticId)
				.WithLocation(2)
				.WithArguments("SelectOneAttribute", "OneMissing");

			var expected3 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidFactoryTypeDiagnosticId)
				.WithLocation(3)
				.WithArguments("Clazz");

			var expected4 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidFactoryTypeDiagnosticId)
				.WithLocation(4)
				.WithArguments("AbstractFactory");

			var expected5 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.MissingFactoryTypeParamDiagnosticId)
				.WithLocation(5)
				.WithArguments("SelectManyAttribute", "ManyMissing");

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11,
				expected0, expected1, expected2, expected3, expected4, expected5);
		}

		[TestMethod]
		public async Task SelectionQueryTypeParameterTest()
		{
			string test = $@"
				using EPiServer.Shell.ObjectEditing;

				namespace tests
				{{
					{GoodISelectionQueryImplementation}

					public class Clazz {{ }} // Bad selection query implementation

					public abstract class AbstractQuery : SelectionQuery {{ }}

					[EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")]
					public class TestPage : EPiServer.Core.PageData
					{{
						[AutoSuggestSelection(typeof(SelectionQuery))]
						public virtual string Good {{ get; set; }}

						[{Highlight(0, "AutoSuggestSelection(typeof(Clazz))")}]
						public virtual string Bad {{ get; set; }}

						[{Highlight(1, "AutoSuggestSelection(typeof(AbstractQuery))")}]
						public virtual string Abstract {{ get; set; }}
					}}
				}}";

			var expected0 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidSelectionQueryTypeDiagnosticId)
				.WithLocation(0)
				.WithArguments("Clazz");

			var expected1 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidSelectionQueryTypeDiagnosticId)
				.WithLocation(1)
				.WithArguments("AbstractQuery");

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0, expected1);
		}

		[TestMethod]
		public async Task CustomSelectionAttribute()
		{
			string test = $@"
				using EPiServer.Shell.ObjectEditing;

				namespace tests
				{{
					{GoodISelectionFactoryImplementation}

					{GoodISelectionQueryImplementation}

					public class Clazz {{ }}

					[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
					public class CustomSelectOneAttribute : SelectOneAttribute
					{{
						public override System.Type SelectionFactoryType
						{{
							get => typeof(SelectionFactory);
							set => base.SelectionFactoryType = value;
						}}
					}}

					[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
					public class CustomSelectManyAttribute : SelectManyAttribute
					{{
						public override System.Type SelectionFactoryType
						{{
							get => typeof(SelectionFactory);
							set => base.SelectionFactoryType = value;
						}}
					}}

					[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
					public class CustomAutoSuggestSelectionAttribute : AutoSuggestSelectionAttribute
					{{
						public CustomAutoSuggestSelectionAttribute() : base(typeof(SelectionQuery))
						{{ }}

						public CustomAutoSuggestSelectionAttribute(System.Type selectionFactoryType) : base(selectionFactoryType)
						{{ }}
					}}

					[EPiServer.DataAnnotations.ContentTypeAttribute(GroupName = ""Content"")]
					public class TestPage : EPiServer.Core.PageData
					{{
						[CustomSelectOne(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual string OneGood {{ get; set; }}

						[{Highlight(0, "CustomSelectOne(SelectionFactoryType = typeof(Clazz))")}]
						public virtual string OneBad {{ get; set; }}

						[CustomSelectOne]
						public virtual string OneMissing {{ get; set; }}

						[CustomSelectMany(SelectionFactoryType = typeof(SelectionFactory))]
						public virtual string ManyGood {{ get; set; }}

						[{Highlight(1, "CustomSelectMany(SelectionFactoryType = typeof(Clazz))")}]
						public virtual string ManyBad {{ get; set; }}

						[CustomSelectMany]
						public virtual string ManyMissing {{ get; set; }}

						[CustomAutoSuggestSelection(typeof(SelectionQuery))]
						public virtual string QueryGood {{ get; set; }}

						[{Highlight(2, "CustomAutoSuggestSelection(typeof(Clazz))")}]
						public virtual string QueryBad {{ get; set; }}

						[CustomAutoSuggestSelection]
						public virtual string QueryMissing {{ get; set; }}
					}}
				}}";

			var expected0 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidFactoryTypeDiagnosticId)
				.WithLocation(0)
				.WithArguments("Clazz");

			var expected1 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidFactoryTypeDiagnosticId)
				.WithLocation(1)
				.WithArguments("Clazz");

			var expected2 = VerifyCS.Diagnostic(SelectionFactoryAnalyzer.InvalidSelectionQueryTypeDiagnosticId)
				.WithLocation(2)
				.WithArguments("Clazz");

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0, expected1, expected2);
		}
	}
}
