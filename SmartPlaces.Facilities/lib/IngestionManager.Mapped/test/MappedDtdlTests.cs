// -----------------------------------------------------------------------
// <copyright file="MappedDtdlTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using DTDLParser;
    using Xunit;

    public class MappedDtdlTests
    {
        [Theory]
        [InlineData("mapped_dtdl.json")]
        public void ValidateDtmisAreValid(string dtdl)
        {
            var parser = new ModelParser();
            var inputDtmi = LoadDtdl(dtdl);
            var inputModels = parser.Parse(inputDtmi);

            Assert.NotEmpty(inputModels);
        }

        private static IEnumerable<string> LoadDtdl(string dtdlFile)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(dtdlFile));
            List<string> dtdls = new List<string>();

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        dtdls.Add(result);
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourceName);
                }
            }

            return dtdls;
        }
    }
}