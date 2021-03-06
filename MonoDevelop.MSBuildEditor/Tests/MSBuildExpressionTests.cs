﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NUnit.Framework;
using MonoDevelop.MSBuildEditor.Language;
using System.Linq;

namespace MonoDevelop.MSBuildEditor.Tests
{
	[TestFixture]
	class MSBuildExpressionTests
	{
		[TestCase ("$(Foo)", "Foo")]
		[TestCase ("$(   Foo   )", "Foo")]
		[TestCase ("$(_Foo)", "_Foo")]
		[TestCase ("$(_Foo12_3)", "_Foo12_3")]
		public void TestSimpleProperty (string expression, string propName)
		{
			var expr = ExpressionParser.Parse (expression);
			var prop = AssertCast<ExpressionProperty> (expr);
			Assert.IsTrue (prop.IsSimpleProperty);
			Assert.AreEqual (propName, prop.Name);
		}

		[TestCase ("$(", ExpressionErrorKind.ExpectingPropertyName)]
		[TestCase ("$(    ", ExpressionErrorKind.ExpectingPropertyName)]
		[TestCase ("@(", ExpressionErrorKind.ExpectingItemName)]
		[TestCase ("%(", ExpressionErrorKind.ExpectingMetadataOrItemName)]
		[TestCase ("$(.", ExpressionErrorKind.ExpectingPropertyName)]
		[TestCase ("$(   .", ExpressionErrorKind.ExpectingPropertyName)]
		[TestCase ("@(.", ExpressionErrorKind.ExpectingItemName)]
		[TestCase ("%(.", ExpressionErrorKind.ExpectingMetadataOrItemName)]
		[TestCase ("$(a", ExpressionErrorKind.ExpectingRightParenOrPeriod)]
		[TestCase ("@(a", ExpressionErrorKind.ExpectingRightParenOrDash)]
		[TestCase ("%(a", ExpressionErrorKind.ExpectingRightParenOrPeriod)]
		[TestCase ("$(a-", ExpressionErrorKind.ExpectingRightParenOrPeriod)]
		[TestCase ("@(a.", ExpressionErrorKind.ExpectingRightParenOrDash)]
		[TestCase ("%(a-", ExpressionErrorKind.ExpectingRightParenOrPeriod)]
		[TestCase ("%(a.b", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("%(a.b.", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("%(a.", ExpressionErrorKind.ExpectingMetadataName)]
		[TestCase ("%(a.)", ExpressionErrorKind.ExpectingMetadataName)]
		[TestCase ("@(a-", ExpressionErrorKind.ExpectingRightAngleBracket)]
		[TestCase ("@(a   -", ExpressionErrorKind.ExpectingRightAngleBracket)]
		[TestCase ("@(a-.", ExpressionErrorKind.ExpectingRightAngleBracket)]
		[TestCase ("@(a->", ExpressionErrorKind.ExpectingApos)]
		[TestCase ("@(a->  ", ExpressionErrorKind.ExpectingApos)]
		[TestCase ("@(a->.", ExpressionErrorKind.ExpectingApos)]
		[TestCase ("@(a->'f", ExpressionErrorKind.ExpectingApos)]
		[TestCase ("@(a->''", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("@(a->''d", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("@(a->'' ", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("$(a.", ExpressionErrorKind.ExpectingMethodName)]
		[TestCase ("$(a..", ExpressionErrorKind.ExpectingMethodName)]
		[TestCase ("$(a.b", ExpressionErrorKind.ExpectingLeftParen)]
		[TestCase ("$(a.b.", ExpressionErrorKind.ExpectingLeftParen)]
		[TestCase ("$(a.b(", ExpressionErrorKind.ExpectingRightParenOrValue)]
		[TestCase ("$(a.b(.", ExpressionErrorKind.IncompleteValue)]
		[TestCase ("$(a.b(/", ExpressionErrorKind.ExpectingRightParenOrValue)]
		[TestCase ("$(a.b()", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("$(a.b().", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("$(a.b(true,", ExpressionErrorKind.ExpectingValue)]
		[TestCase ("$(a.b(true,   ", ExpressionErrorKind.ExpectingValue)]
		[TestCase ("$(a.b(true,.", ExpressionErrorKind.IncompleteValue)]
		[TestCase ("$(a.b(true,/", ExpressionErrorKind.ExpectingValue)]
		[TestCase ("$(a.b(true,   .", ExpressionErrorKind.IncompleteValue)]
		[TestCase ("$(a.b(true,   /", ExpressionErrorKind.ExpectingValue)]
		[TestCase ("$(a.b(true,true", ExpressionErrorKind.ExpectingRightParenOrComma)]
		[TestCase ("$(a.b(true,true)", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("$(a.b(true,true)   ", ExpressionErrorKind.ExpectingRightParen)]
		[TestCase ("$(a.b(true,true)   _", ExpressionErrorKind.ExpectingRightParen)]
		public void TestSimpleError (string expression, ExpressionErrorKind error)
		{
			var expr = ExpressionParser.Parse (expression, ExpressionOptions.Metadata);
			var err = AssertCast<ExpressionError> (expr);
			Assert.AreEqual (error, err.Kind);
		}

		[TestCase ("$")]
		[TestCase ("@")]
		[TestCase ("%")]
		[TestCase ("$a")]
		[TestCase ("@b")]
		[TestCase ("%c")]
		[TestCase ("$ ")]
		[TestCase ("@ ")]
		[TestCase ("% ")]
		public void TestLiteral (string expression)
		{
			var expr = ExpressionParser.Parse (expression, ExpressionOptions.Metadata);
			var lit = AssertCast<ExpressionText> (expr);
			Assert.AreEqual (expression, lit.Value);
		}

		[Test]
		public void TestMetadataDisallowed ()
		{
			var expr = ExpressionParser.Parse ("%(Foo)", ExpressionOptions.None);
			var err = AssertCast<ExpressionError> (expr);
			Assert.AreEqual (err.Kind, ExpressionErrorKind.MetadataDisallowed);
		}

		[Test]
		public void TestItemsDisallowed ()
		{
			var expr = ExpressionParser.Parse ("@(Foo)", ExpressionOptions.None);
			var err = AssertCast<ExpressionError> (expr);
			Assert.AreEqual (err.Kind, ExpressionErrorKind.ItemsDisallowed);
		}

		[TestCase ("@(Foo)", "Foo")]
		[TestCase ("@(_Foo)", "_Foo")]
		[TestCase ("@(_Foo12_3)", "_Foo12_3")]
		public void TestItem (string expression, string itemName)
		{
			var expr = ExpressionParser.Parse (expression, ExpressionOptions.Items);
			var item = AssertCast<ExpressionItem> (expr);
			Assert.AreEqual (itemName, item.Name);
		}

		[TestCase ("%(Foo)", "Foo")]
		[TestCase ("%(_Foo)", "_Foo")]
		[TestCase ("%(_Foo12_3)", "_Foo12_3")]
		public void TestUnqualifiedMetadata (string expression, string metaName)
		{
			var expr = ExpressionParser.Parse (expression, ExpressionOptions.Metadata);
			var meta = AssertCast<ExpressionMetadata> (expr);
			Assert.AreEqual (metaName, meta.MetadataName);
			Assert.IsNull (meta.ItemName);
		}

		[TestCase ("%(Foo.Bar)", "Foo", "Bar")]
		[TestCase ("%(_Foo._Bar)", "_Foo", "_Bar")]
		[TestCase ("%(_Foo12_3._Bar3_4)", "_Foo12_3", "_Bar3_4")]
		public void TestQualifiedMetadata (string expression, string itemName, string metaName)
		{
			var expr = ExpressionParser.Parse (expression, ExpressionOptions.Metadata);
			var meta = AssertCast<ExpressionMetadata> (expr);
			Assert.AreEqual (metaName, meta.MetadataName);
			Assert.AreEqual (itemName, meta.ItemName);
		}

		[Test]
		public void TestPropertyLiteral ()
		{
			TestParse (
				"abc$(Foo)cde@(baritem)510",
				 new Expression (
					0, 25,
					new ExpressionText (0, "abc", false),
					new ExpressionProperty (3, 6, "Foo"),
					new ExpressionText (9, "cde", false),
					new ExpressionItem (12, 10, "baritem"),
					new ExpressionText (22, "510", false)
				),
				ExpressionOptions.Items
			);
		}

		[Test]
		public void TestList ()
		{
			TestParse (
				"abc;$(Foo)cde;@(baritem);stuff",
				 new ExpressionList (
					0, 30,
					new ExpressionText (0, "abc", true),
					new Expression (
						4, 9,
						new ExpressionProperty (4, 6, "Foo"),
						new ExpressionText (10, "cde", false)
					),
					new ExpressionItem (14, 10, "baritem"),
					new ExpressionText (25, "stuff", true)
				),
				ExpressionOptions.ItemsAndLists
			);
		}

		[Test]
		public void TestNoLists ()
		{
			TestParse (
				"abc;$(Foo)",
				 new Expression (
					0, 10,
					new ExpressionText (0, "abc;", false),
					new ExpressionProperty (4, 6, "Foo")
				)
			);
		}

		[Test]
		public void TestItemTransform ()
		{
			TestParse (
				"@(Foo->'%(Bar.Baz)')",
				new ExpressionItem (
					0, 20,
					"Foo",
					new ExpressionMetadata (8, 10, "Bar", "Baz")
				),
				ExpressionOptions.Items
			);
		}

		[Test]
		public void TestBaseOffset ()
		{
			TestParse (
				"$(a) $(b);$(c);d",
				new ExpressionList (
					100, 16,
					new Expression (
						100, 9,
						new ExpressionProperty (100, 4, "a"),
						new ExpressionText (104, " ", false),
						new ExpressionProperty (105, 4, "b")
					),
					new ExpressionProperty (110, 4, "c"),
					new ExpressionText (115, "d", true)
				),
				ExpressionOptions.Lists,
				100
			);
		}

		[Test]
		public void TestXmlEntities ()
		{
			TestParse (
				"&quot;;d&foo;bar",
				new ExpressionList (
					0, 16,
					new ExpressionText (0, "&quot;", true),
					new ExpressionText (7, "d&foo;bar", true)
				),
				ExpressionOptions.Lists
			);
		}

		[TestCase ("$(Foo.Bar())", "Foo", "Bar")]
		[TestCase ("$(   Foo  .  Bar  (  )  )", "Foo", "Bar")]
		//[TestCase ("$(Foo.Baz('Hello'))", "Foo", "Baz", "Hello")]
		[TestCase ("$(Foo.A(5))", "Foo", "A", 5)]
		[TestCase ("$(Foo.A(true))", "Foo", "A", true)]
		[TestCase ("$(Foo.A(true,   20 ))", "Foo", "A", true, 20)]
		[TestCase ("$(Foo.A(20.5))", "Foo", "A", 20.5d)]
		[TestCase ("$(Foo.A(.61))", "Foo", "A", .61d)]
		public void TestSimpleStringFunctions(object[] args)
		{
			var expr = ExpressionParser.Parse ((string)args[0], ExpressionOptions.None, 0);
			var targetName = (string)args [1];
			var methodName = (string)args [2];
			var methArgs = args.Skip (3).ToList ();

			var prop = AssertCast<ExpressionProperty> (expr);
			Assert.IsFalse (prop.IsSimpleProperty);

			var invocation = AssertCast<ExpressionPropertyFunctionInvocation> (prop.Expression);
			var target = AssertCast<ExpressionPropertyName> (invocation.Target);

			Assert.AreEqual (targetName, target.Name);
			Assert.AreEqual (methodName, invocation.MethodName);

			Assert.AreEqual (methArgs.Count, invocation.Arguments.Arguments.Count);
			for (int i = 0; i < methArgs.Count; i++) {
				var arg = AssertCast<ExpressionArgumentLiteral> (invocation.Arguments.Arguments [i]);
				Assert.AreEqual (methArgs[i], arg.Value);
			}
		}

		static void TestParse (string expression, ExpressionNode expected, ExpressionOptions options = ExpressionOptions.None, int baseOffset = 0)
		{
			var expr = ExpressionParser.Parse (expression, options, baseOffset);
			AssertEqual (expected, expr);
		}

		static void AssertEqual (ExpressionNode expected, ExpressionNode actual)
		{
			Assert.That (actual, Is.TypeOf (expected.GetType ()));
			switch (actual)
			{
			case Expression expr:
				var expectedExpr = (Expression)expected;
				Assert.AreEqual (expectedExpr.Nodes.Count, expr.Nodes.Count);
				for (int i = 0; i < expr.Nodes.Count; i++) {
					AssertEqual (expectedExpr.Nodes [i], expr.Nodes [i]);
				}
				break;
			case ExpressionText literal:
				var expectedLit = (ExpressionText)expected;
				Assert.AreEqual (expectedLit.Value, literal.Value);
				Assert.AreEqual (expectedLit.IsPure, literal.IsPure);
				break;
			case ExpressionProperty prop:
				Assert.AreEqual (((ExpressionProperty)expected).Name, prop.Name);
				break;
			case ExpressionItem item:
				Assert.AreEqual (((ExpressionItem)expected).Name, item.Name);
				break;
			case ExpressionMetadata meta:
				var expectedMeta = (ExpressionMetadata)expected;
				Assert.AreEqual (expectedMeta.MetadataName, meta.MetadataName);
				Assert.AreEqual (expectedMeta.ItemName, meta.ItemName);
				break;
			default:
				throw new Exception ("Unsupported node kind");
			}
			Assert.AreEqual (expected.Length, actual.Length);
			Assert.AreEqual (expected.Offset, actual.Offset);
		}

		static T AssertCast<T> (object o)
		{
			Assert.IsInstanceOf<T> (o);
			return (T)o;
		}
	}
}