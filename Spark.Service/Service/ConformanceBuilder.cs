﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Support;
using System.IO;
using Hl7.Fhir.Serialization;
using System.Xml.Linq;
using Hl7.Fhir.Rest;

namespace Spark.Service
{
    public static class ConformanceBuilder
    {
        public const string CONFORMANCE_ID = "self";
        public static readonly string CONFORMANCE_COLLECTION_NAME = typeof(Conformance).GetCollectionName();

        public static Conformance Build()
        {
            Stream s = typeof(ConformanceBuilder).Assembly.GetManifestResourceStream("Spark.Service.Service.Conformance.xml");
            StreamReader sr = new StreamReader(s);
            string conformanceXml = sr.ReadToEnd();


            var conformance = (Conformance)FhirParser.ParseResourceFromXml(conformanceXml);

            conformance.Software.Version = ReadVersionFromAssembly();
            conformance.Software.Name = ReadProductNameFromAssembly();
            conformance.FhirVersion = ModelInfo.Version;
            conformance.Date = Date.Today().Value;

            Conformance.ConformanceRestComponent serverComponent = conformance.Rest[0];

            // Replace anything that was there before...
            serverComponent.Resource = new List<Conformance.ConformanceRestResourceComponent>();

            // todo: An alternative is needed for the missing operation types below:
            var allOperations = new List<Conformance.ResourceInteractionComponent>()
            {   new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.Create },
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.Delete },
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.HistoryInstance },
                /*
                new Conformance.ConformanceRestResourceOperationComponent { Code =
                    Conformance.RestfulOperationType.HistorySystem },
                */
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.HistoryType },
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.Read },
                
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.SearchType },
                /*
                    new Conformance.ConformanceRestResourceOperationComponent { Code =
                    Conformance.RestfulOperationType.Transaction },
                */
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.Update },
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.Validate },
                new Conformance.ResourceInteractionComponent { Code =
                    Conformance.TypeRestfulInteraction.Vread },
            };

            foreach (var resourceName in ModelInfo.SupportedResources)
            {
                var supportedResource = new Conformance.ConformanceRestResourceComponent();
                supportedResource.Type = resourceName;
                supportedResource.ReadHistory = true;
                supportedResource.Interaction = allOperations;

                // Add supported _includes for this resource
                supportedResource.SearchInclude = ModelInfo.SearchParameters
                    .Where(sp => sp.Resource == resourceName)
                    .Where(sp => sp.Type == Conformance.SearchParamType.Reference)
                    .Select(sp => sp.Name);

                supportedResource.SearchParam = new List<Conformance.ConformanceRestResourceSearchParamComponent>();

                // todo: search params. error: "The name "Search" does not exist in the current context
                var parameters = ModelInfo.SearchParameters.Where(sp => sp.Resource == resourceName)
                        .Select(sp => new Conformance.ConformanceRestResourceSearchParamComponent
                        {
                            Name = sp.Name,
                            Definition = new Uri("http://hl7.org/fhir/" + resourceName.ToLower() + "/search#" + sp.Name).OriginalString,
                            Type = sp.Type,
                            Documentation = sp.Description,
                            Chain = sp.Path // != null ? String.Join(",", sp.Path) : null
                        });

                supportedResource.SearchParam.AddRange(parameters);

                serverComponent.Resource.Add(supportedResource);
            }

            // todo: This constant has become internal. Please undo. We need it. 

            // Update: new location: XHtml.XHTMLNS / XHtml
            // XNamespace ns = Hl7.Fhir.Support.Util.XHTMLNS;
            XNamespace ns = "http://www.w3.org/1999/xhtml";

            var summary = new XElement(ns + "div");

            foreach (string resourceName in ModelInfo.SupportedResources)
            {
                summary.Add(new XElement(ns + "p",
                    String.Format("The server supports all operations on the {0} resource, including history",
                        resourceName)));
            }

            conformance.Text.Div = summary.ToString();
            return conformance;
        }


        public static string ReadVersionFromAssembly()
        {
            var attribute = (System.Reflection.AssemblyFileVersionAttribute)typeof(ConformanceBuilder).Assembly
                .GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), true)
                .Single();
            return attribute.Version;
        }

        public static string ReadProductNameFromAssembly()
        {
            var attribute = (System.Reflection.AssemblyProductAttribute)typeof(ConformanceBuilder).Assembly
                .GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), true)
                .Single();
            return attribute.Product;
        }
    }

}