// -----------------------------------------------------------------------
// <copyright file="MappingFileException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OntologyMapper
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
