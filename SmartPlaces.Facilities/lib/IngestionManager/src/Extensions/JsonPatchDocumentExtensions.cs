//-----------------------------------------------------------------------
// <copyright file="JsonPatchDocumentExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions
{
    using global::Azure;

    /// <summary>
    /// Static extension method class for <see cref="JsonPatchDocument"/> extensions.
    /// </summary>
    internal static class JsonPatchDocumentExtensions
    {
        /// <summary>
        /// Checks if a JSON Patch is empty of content or operations.
        /// </summary>
        /// <param name="document">JSON Patch to check.</param>
        /// <returns><c>true</c> if input document is null, whitespace, a zero-length JSON array, or an empty JSON object, else <c>false</c>.</returns>
        public static bool IsEmpty(this JsonPatchDocument document)
        {
            var doc = document.ToString();
            if (string.IsNullOrWhiteSpace(doc))
            {
                return true;
            }

            if (doc == "[]" || doc == "{}")
            {
                return true;
            }

            return false;
        }
    }
}
