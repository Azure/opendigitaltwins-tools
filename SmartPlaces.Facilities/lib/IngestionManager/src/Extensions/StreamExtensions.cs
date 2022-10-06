//-----------------------------------------------------------------------
// <copyright file="StreamExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IngestionManager.Extensions
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;

    public static partial class StreamExtensions
    {
        // Taken from: https://msazure.visualstudio.com/One/_git/Azure-IoT-DigitalTwins-Main?path=/Tools/NDJsonGenerator/NDJsonGenerator.cs
        public static void ToNewlineDelimitedJson<T>(this Stream stream, IEnumerable<T> items)
        {
            var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            foreach (var item in items)
            {
                if (item != null)
                {
                    string sanitizedTwin = JsonSerializer.Serialize(item, item.GetType()).Replace(Environment.NewLine, string.Empty);
                    streamWriter.WriteLine(sanitizedTwin);
                }
            }

            streamWriter.Flush();
        }
    }
}
