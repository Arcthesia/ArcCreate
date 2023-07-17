// Credit: https://stackoverflow.com/questions/754233/is-it-there-any-lru-implementation-of-idictionary#3719378

namespace ArcCreate.Utility.LRUCache
{
    public class LRUCacheItem<K, V>
    {
        public LRUCacheItem(K k, V v)
        {
            Key = k;
            Value = v;
        }

        public K Key { get; set; }

        public V Value { get; set; }
    }
}