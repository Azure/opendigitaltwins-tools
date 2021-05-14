using Microsoft.Azure.DigitalTwins.Parser;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UploadModels
{
    internal class DTInterfaceInfoEqualityComparer : IEqualityComparer<DTInterfaceInfo>
    {
        public bool Equals([AllowNull] DTInterfaceInfo x, [AllowNull] DTInterfaceInfo y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
            {
                return true;
            }
            else if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }
            else
            {
                return x.Id.Equals(y.Id);
            }
        }

        public int GetHashCode([DisallowNull] DTInterfaceInfo obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
