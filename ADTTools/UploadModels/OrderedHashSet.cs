using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UploadModels
{
    internal class OrderedHashSet<T> : KeyedCollection<T, T>
    {
        public OrderedHashSet()
        {
        }

        public OrderedHashSet(IEqualityComparer<T> comparer)
            : base(comparer)
        {
        }

        protected override T GetKeyForItem(T item)
        {
            return item;
        }
    }
}
