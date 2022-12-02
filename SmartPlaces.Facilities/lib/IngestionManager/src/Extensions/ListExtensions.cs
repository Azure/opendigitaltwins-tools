//-----------------------------------------------------------------------
// <copyright file="ListExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Static extension method class for <see cref="IList{T}"/> extensions.
    /// </summary>
    internal static class ListExtensions
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        /// Shuffles the order of the list elements randomly.
        /// </summary>
        /// <typeparam name="T">Type of elements in list.</typeparam>
        /// <param name="list">List to shuffle.</param>
        /// <returns>Shuffled list.</returns>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }

            return list;
        }
    }
}
