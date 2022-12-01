// -----------------------------------------------------------------------
// <copyright file="MappingFileException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    /// <summary>
    /// Defines the methods to be implemented by an OntologyMappingManager.
    /// </summary>
    public class MappingFileException : Exception
    {
        private readonly string filename;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingFileException"/> class.
        /// </summary>
        /// <param name="message">Text describing what went wrong.</param>
        /// <param name="filename">The file that caused this error.</param>
        public MappingFileException(string message, string filename)
            : base(message)
        {
            this.filename = filename;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingFileException"/> class.
        /// </summary>
        /// <param name="message">Text describing what went wrong.</param>
        /// <param name="filename">The file that caused this error.</param>
        /// <param name="innerException">Nested inner exception that triggered this exception.</param>
        public MappingFileException(string message, string filename, Exception innerException)
            : base(message, innerException)
        {
            this.filename = filename;
        }

        /// <summary>
        /// Gets the file that caused this error.
        /// </summary>
        public string Filename => filename;
    }
}