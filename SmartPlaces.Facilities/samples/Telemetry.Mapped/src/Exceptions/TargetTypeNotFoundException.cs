//-----------------------------------------------------------------------
// <copyright file="TargetTypeNotFoundException.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry.Exceptions
{
    using System;

    public class TargetTypeNotFoundException : Exception
    {
        public TargetTypeNotFoundException(string message)
            : base(message)
        {
        }
    }
}
