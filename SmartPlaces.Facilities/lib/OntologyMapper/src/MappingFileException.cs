// -----------------------------------------------------------------------
// <copyright file="MappingFileException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    /// <summary>
    /// Defines OntologyMapper-specific exceptions.
    /// </summary>
    public class MappingFileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingFileException"/> class.
        /// </summary>
        /// <param name="message">Text describing what went wrong.</param>
        /// <param name="filename">The file that caused this error.</param>
        public MappingFileException(string message, string filename)
            : base(message)
        {
            Filename = filename;
        }

        /// <summary>
        /// Gets the file that caused this error.
        /// </summary>
        public string Filename { get; }
    }
}
