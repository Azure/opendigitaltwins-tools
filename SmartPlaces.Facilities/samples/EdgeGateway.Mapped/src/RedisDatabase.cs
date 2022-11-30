// -----------------------------------------------------------------------
// <copyright file="RedisDatabase.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace EdgeGateway
{
    /// <summary>
    /// Redis internally has different databases you can take advantage of, this enum 
    /// defines the agreed upon database index to use between Mapped and EdgeGateway.
    /// </summary>
    internal enum RedisDatabase
    {
        Telemetry = 0
    }
}
