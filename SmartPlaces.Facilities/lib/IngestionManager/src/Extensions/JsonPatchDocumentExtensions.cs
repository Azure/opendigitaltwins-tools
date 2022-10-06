//-----------------------------------------------------------------------
// <copyright file="JsonPatchDocumentExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions
{
    using global::Azure;

    internal static class JsonPatchDocumentExtensions
    {
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
