﻿/* 
 * Copyright (c) 2015, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.FluentPath;
using System.Diagnostics;
using Hl7.Fhir.FluentPath.InstanceTree;
using Hl7.Fhir.Navigation;
using HL7.Fhir.FluentPath;

namespace Hl7.Fhir.Tests.FhirPath
{
    [TestClass]
#if PORTABLE45
	public class PortableFhirPathEvaluatorTest
#else
    public class FhirPathEvaluatorTest
#endif
    {
        FhirInstanceTree tree;
        IEnumerable<IValueProvider> testInput;

        [TestInitialize]
        public void Setup()
        {
            var tpXml = System.IO.File.ReadAllText("TestData\\FhirPathTestResource.xml");
            tree = TreeConstructor.FromXml(tpXml);
            testInput = FhirValueList.Create(new TreeNavigator(tree));
        }


        [TestMethod, TestCategory("FhirPath")]
        public void TestTreeVisualizerVisitor()
        {
            var expr = PathExpression.Parse("doSomething('ha!', 4, {}, $this, somethingElse(true))");
            var result = expr.Dump();
            Debug.WriteLine(result);
        }

        [TestMethod, TestCategory("FhirPath")]
        public void TestExistence()
        {
            Assert.IsTrue(PathExpression.IsTrue(@"{}.empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"1.empty().not()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"1.exists()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"Patient.identifier.exists()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"Patient.dientifeir.exists().not()", testInput));
            Assert.AreEqual(2L, PathExpression.Scalar(@"Patient.identifier.count()", testInput));
        }


        [TestMethod, TestCategory("FhirPath")]
        public void TestDynaBinding()
        {
            var input = FhirValueList.Create(new ConstantValue("Hello world!"), new ConstantValue(4));
            //Assert.AreEqual("ello", PathExpression.Scalar(@"$this[0].substring(1, $this[1])", input));
            Assert.AreEqual("ello", PathExpression.Scalar(@"first().substring(1, %context.skip(1))", input));
        }



        [TestMethod, TestCategory("FhirPath")]
        public void TestMath()
        {
            //            Assert.AreEqual(-1.5, PathExpression.Scalar(@"3 * -0.5", navigator));
            Assert.IsTrue(PathExpression.IsTrue(@"-4.5 + 4.5 = 0", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"4/2 = 2", testInput));
            //            Assert.IsTrue(PathExpression.IsTrue(@"2/4 = 0.5", navigator));
            Assert.IsTrue(PathExpression.IsTrue(@"10/4 = 2", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"10.0/4 = 2.5", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"4.0/2.0 = 2", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"2.0/4 = 0.5", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"2.0 * 4 = 8", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"2 * 4.1 = 8.2", testInput));
//            Assert.IsTrue(PathExpression.IsTrue(@"-0.5 * -0.5 = -0.25", navigator));
            Assert.IsTrue(PathExpression.IsTrue(@"5 - 4.5 = 0.5", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"9.5 - 4.5 = 5", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"5 + 4.5 = 9.5", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"9.5 + 0.5 = 10", testInput));

            Assert.IsTrue(PathExpression.IsTrue(@"103 mod 5 = 3", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"101.4 mod 5.2 = 2.6", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"103 div 5 = 20", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"20.0 div 5.5 = 3", testInput));

            Assert.IsTrue(PathExpression.IsTrue(@"'offic'+'ial' = 'official'", testInput));

            Assert.IsTrue(PathExpression.IsTrue(@"12/(2+2) - (3 div 2) = 2", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"-4.5 + 4.5 * 2 * 4 / 4 - 1.5 = 3", testInput));
        }


        [TestMethod, TestCategory("FhirPath")]
        public void Test3VLBoolean()
        {
            Assert.IsTrue(PathExpression.IsTrue(@"true and true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(true and false) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(true and {}).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false and true) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false and false) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false and {}) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} and true).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} and false) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} and {}).empty()", testInput));

            Assert.IsTrue(PathExpression.IsTrue(@"true or true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"true or false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"true or {}", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"false or true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false or false) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false or {}).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"{} or true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} or false).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} or {}).empty()", testInput));

            Assert.IsTrue(PathExpression.IsTrue(@"(true xor true)=false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"true xor false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(true xor {}).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"false xor true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false xor false) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false xor {}).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} xor true).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} xor false).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} xor {}).empty()", testInput));

            Assert.IsTrue(PathExpression.IsTrue(@"true implies true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(true implies false) = false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(true implies {}).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"false implies true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"false implies false", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"false implies {}", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"{} implies true", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} implies false).empty()", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"({} implies {}).empty()", testInput));
        }

        [TestMethod, TestCategory("FhirPath")]
        public void TestLogicalShortcut()
        {
            Assert.IsTrue(PathExpression.IsTrue(@"true or (1/0 = 0)", testInput));
            Assert.IsTrue(PathExpression.IsTrue(@"(false and (1/0 = 0)) = false", testInput));
        }

        [TestMethod, TestCategory("FhirPath")]
        public void TestExpression()
        {            
            Assert.IsTrue(PathExpression.IsTrue(@"(Patient.identifier.where( use = ( 'offic' + 'ial')) = 
                       Patient.identifier.skip(8/2 - 3*2 + 3)) and (Patient.identifier.where(use='usual') = 
                        Patient.identifier.first())", testInput));

            //// xpath gebruikt $current for $focus....waarom dat niet gebruiken?
            //Assert.IsTrue(PathExpression.IsTrue(
            //      @"Patient.contact.relationship.coding.where($focus.system = %vs-patient-contact-relationship and 
            //            $focus.code = 'owner').log('after owner').$parent.$parent.organization.log('org')
            //            .where(display.startsWith('Walt')).resolve().identifier.first().value = 'Gastro'", navigator,
            //                    new TestEvaluationContext()));

            ////// why is in an operator and not a function?
            //Assert.IsTrue(PathExpression.IsTrue(
            //     @"(Patient.identifier.where(use='official') in Patient.identifier) and
            //           (Patient.identifier.first() in Patient.identifier.tail()).not()", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //    @"(1|2|2|3|Patient.identifier.first()|Patient.identifier).distinct().count() = 
            //            3 + Patient.identifier.count()", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //            @"(1|2|3|4|5).where($focus > 2 and $focus <= 4) = (3|4)", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //            @"Patient.name.select(given|family).count() = 2", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //        @"Patient.**.contains('wne') = contact.relationship.coding.code and
            //        Patient.**.matches('i.*/gif') in Patient.photo.*", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //    @"'m' + gender.extension('http://example.org/StructureDefinition/real-gender').valueCode
            //        .substring(1,4) + 
            //        gender.extension('http://example.org/StructureDefinition/real-gender').valueCode
            //        .substring(5) = 'metrosexual'", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //        @"Patient.identifier.any(use='official') and identifier.where(use='usual').any()", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //        @"gender.extension('http://example.org/StructureDefinition/real-gender').valueCode
            //        .select('m' + $focus.substring(1,4) + $focus.substring(5)) = 'metrosexual'", navigator));

            //Assert.IsTrue(PathExpression.IsTrue(
            //        @"Patient.**.where($focus.contains('222')).item(1) = $context.contained.address.line", navigator));
        }

        //[TestMethod, TestCategory("FhirPath")]
        //public void TestExpressionTodayFunction()
        //{
        //    // Check that date comes in
        //    Assert.IsTrue(PartialDateTime.Parse(DateTime.Today.ToFhirDate()).ToString(), PathExpression.Evaluate("today()", tree).AsDateTime().ToString());

        //    // Check greater than
        //    Assert.IsTrue(PathExpression.Predicate("today() < " + DateTime.Today.AddDays(1).ToFhirDate(), tree));

        //    // Check less than
        //    Assert.IsTrue(PathExpression.Predicate("today() > " + DateTime.Today.AddDays(-1).ToFhirDate(), tree));

        //    // Check ==
        //    Assert.IsTrue(PathExpression.Predicate("today() = " + DateTime.Today.ToFhirDate(), tree));
        //}

        [TestMethod, TestCategory("FhirPath")]
        public void TestExpressionSubstringFunction()
        {
            // Check that date comes in
            //Assert.IsTrue(PathExpression.IsTrue("QuestionnaireResponse.group.group.where(linkId=\"Section - C\").question.where(linkId=\"C1\").answer.group.where(linkId = \"C1fields\").question.where(linkId = \"DateReturnToNormalDuties\").answer.valueDate.empty()", navigator));

            //Assert.IsFalse(PathExpression.IsTrue("QuestionnaireResponse.group.group.where(linkId=\"Section - C\").question.where(linkId=\"C1\").answer.group.where(linkId = \"C1fields\").question.where(linkId = \"DateReturnToNormalDuties\").answer.valueDate.empty().not()", navigator));

            // Assert.AreEqual("1973-05-31", PathExpression.Evaluate("Patient.contained.Patient.birthDate.substring(0,10)", tree).ToString());

            // Assert.AreEqual(null, PathExpression.Evaluate("Patient.birthDate2", tree).ToString());

            // Assert.AreEqual(null, PathExpression.Evaluate("Patient.birthDate2.substring(0,10)", tree).ToString());
        }

        [TestMethod, TestCategory("FhirPath")]
        public void TestExpressionRegexFunction()
        {
            // Check that date comes in
            //Assert.IsTrue(PathExpression.IsTrue("Patient.identifier.where(system=\"urn:oid:0.1.2.3.4.5.6.7\").value.matches(\"^[1-6]+$\")", navigator));

            //Assert.IsFalse(PathExpression.IsTrue("Patient.identifier.where(system=\"urn:oid:0.1.2.3.4.5.6.7\").value.matches(\"^[1-3]+$\")", navigator));

            // Assert.AreEqual("1973-05-31", PathExpression.Evaluate("Patient.contained.Patient.birthDate.substring(0,10)", tree).ToString());

            // Assert.AreEqual(null, PathExpression.Evaluate("Patient.birthDate2", tree).ToString());

            // Assert.AreEqual(null, PathExpression.Evaluate("Patient.birthDate2.substring(0,10)", tree).ToString());
        }

        [TestMethod, TestCategory("FhirPath")]
        public void TestExpression2()
        {
            //var result = Grammar.Expr.TryParse("Patient.deceased[x]");

            //if (result.WasSuccessful)
            //{
            //    var evaluator = result.Value;
            //    var resultNodes = evaluator.Select(tree);
            //    Assert.AreEqual(1, resultNodes.Count());
            //}
            //else
            //{
            //    Debug.WriteLine("Expectations: " + String.Join(",", result.Expectations));
            //    Assert.Fail(result.ToString());
            //}
        }

    }
}