// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    /// <summary>
    /// Defines the methods to be implemented by an OntologyMappingManager
    /// </summary>
    public class MappingFileException : Exception
    {
        private readonly string filename;

        public MappingFileException(string message, string filename)
            : base(message)
        {
            this.filename = filename;
        }

        public string Filename => filename;
    }
}
