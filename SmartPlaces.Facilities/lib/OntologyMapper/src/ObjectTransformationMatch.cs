// -----------------------------------------------------------------------
// <copyright file="ObjectTransformationMatch.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    /// <summary>
    /// Indicates what type of match has occurred if any.
    /// </summary>
    public enum ObjectTransformationMatch
    {
        /// <summary>
        /// There is no match on the property passed in at all. No use testing further by type
        /// </summary>
        NoPropertyMatch,

        /// <summary>
        /// There is a match on the property name, but not on the type. Try ancestors
        /// </summary>
        PropertyMatchOnly,

        /// <summary>
        /// There is a match on the property name and the type. We have a match
        /// </summary>
        PropertyAndTypeMatch,
    }
}
