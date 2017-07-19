﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;

namespace Hl7.Fhir.Tests.Rest
{
    [TestClass]
    public class OperationsTests

    {
        string testEndpoint = FhirClientTests.testEndpoint.OriginalString;

        [TestMethod] 
        [TestCategory("IntegrationTest")]
        public void InvokeTestPatientGetEverything()
        {
            var client = new FhirClient(testEndpoint);
            var start = new FhirDateTime(2014,11,1);
            var end = new FhirDateTime(2015,1,1);
            var par = new Parameters().Add("start", start).Add("end", end);
            var bundle = (Bundle)client.InstanceOperation(ResourceIdentity.Build("Patient", "example"), "everything", par);
            Assert.IsTrue(bundle.Entry.Any());

            var bundle2 = client.FetchPatientRecord(ResourceIdentity.Build("Patient","example"), start, end);
            Assert.IsTrue(bundle2.Entry.Any());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void InvokeExpandExistingValueSet()
        {
            var client = new FhirClient(testEndpoint);
            var vs = client.ExpandValueSet(ResourceIdentity.Build("ValueSet","administrative-gender"));
            
            Assert.IsTrue(vs.Expansion.Contains.Any());
        }



        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void InvokeExpandParameterValueSet()
        {
            var client = new FhirClient(testEndpoint);

            var vs = client.Read<ValueSet>("ValueSet/administrative-gender");

            var vsX = client.ExpandValueSet(vs);

            Assert.IsTrue(vsX.Expansion.Contains.Any());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void InvokeExpandUsingInstanceOp()
        {
            var client = new FhirClient("http://sqlonfhir-dstu2.azurewebsites.net/fhir"); // testEndpoint);

            //    var vs = client.Read<ValueSet>("ValueSet/administrative-gender");

            //   var vsX = client.ExpandValueSet(ExpandValueSet(vs);

            // Assert.IsTrue(vsX.Expansion.Contains.Any());
            var result = client.InstanceOperation(ResourceIdentity.Build("ValueSet", "extensional-case-1"),
                FhirClientOperations.Operation.EXPAND_VALUESET);

        }
    

        /// <summary>
        /// http://hl7.org/fhir/valueset-operations.html#lookup
        /// </summary>
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void InvokeLookupCoding()
        {
            var client = new FhirClient("http://sqlonfhir-dstu2.azurewebsites.net/fhir"); // testEndpoint);
            var coding = new Coding("http://hl7.org/fhir/administrative-gender", "male");

            var expansion = client.ConceptLookup(coding);

            // Assert.AreEqual("AdministrativeGender", expansion.GetSingleValue<FhirString>("name").Value); // Returns empty currently on Grahame's server
            Assert.AreEqual("Male", expansion.GetSingleValue<FhirString>("display").Value);               
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void InvokeLookupCode()
        {
            var client = new FhirClient("http://sqlonfhir-dstu2.azurewebsites.net/fhir"); // testEndpoint);
            var expansion = client.ConceptLookup(new Code("male"), new FhirUri("http://hl7.org/fhir/administrative-gender"));

            //Assert.AreEqual("male", expansion.GetSingleValue<FhirString>("name").Value);  // Returns empty currently on Grahame's server
            Assert.AreEqual("Male", expansion.GetSingleValue<FhirString>("display").Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void InvokeValidateCode()
        {
            var client = new FhirClient("http://ontoserver.csiro.au/dstu2_1");
            var coding = new Coding("http://snomed.info/sct", "4322002");

            //var result = client.ValidateCode("http://hl7.org/fhir/ValueSet/c80-facilitycodes", coding);
            var result = client.ValidateCode("c80-facilitycodes", coding, abstractAllowed: new FhirBoolean(false));
            Assert.IsTrue((result.Parameter.Single(p => p.Name == "result")?.Value as FhirBoolean)?.Value == true);

        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void InvokeValidateCodeWithVS()
        {
            var client = new FhirClient("http://ontoserver.csiro.au/dstu2_1");
            var coding = new Coding("http://snomed.info/sct", "4322002");

            var vs = client.Read<ValueSet>("ValueSet/c80-facilitycodes");
            Assert.IsNotNull(vs);

            var result = client.ValidateCode(vs, coding);
            Assert.IsTrue((result.Parameter.Single(p => p.Name == "result")?.Value as FhirBoolean)?.Value == true);
        }


        [TestMethod]//returns 500: validation of slices is not done yet.
        [TestCategory("IntegrationTest")]
        public void InvokeResourceValidation()
        {
            var client = new FhirClient(testEndpoint);

            var pat = client.Read<Patient>("Patient/patient-uslab-example1");

            try
            {
                var vresult = client.ValidateResource(pat, null,
                    new FhirUri("http://hl7.org/fhir/StructureDefinition/uslab-patient"));
                Assert.Fail("Should have resulted in 400");
            }
            catch(FhirOperationException fe)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, fe.Status);
                Assert.IsTrue(fe.Outcome.Issue.Where(i => i.Severity == OperationOutcome.IssueSeverity.Error).Any());
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task InvokeTestPatientGetEverythingAsync()
        {
            string _endpoint = "https://api.hspconsortium.org/rpineda/open";

            var client = new FhirClient(_endpoint);
            var start = new FhirDateTime(2014, 11, 1);
            var end = new FhirDateTime(2020, 1, 1);
            var par = new Parameters().Add("start", start).Add("end", end);

            var bundleTask = client.InstanceOperationAsync(ResourceIdentity.Build("Patient", "SMART-1288992"), "everything", par);
            var bundle2Task = client.FetchPatientRecordAsync(ResourceIdentity.Build("Patient", "SMART-1288992"), start, end);

            await Task.WhenAll(bundleTask, bundle2Task);

            var bundle = (Bundle)bundleTask.Result;
            Assert.IsTrue(bundle.Entry.Any());

            var bundle2 = (Bundle)bundle2Task.Result;
            Assert.IsTrue(bundle2.Entry.Any());
        }
    }
}
