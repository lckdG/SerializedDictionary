using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [System.Serializable]
    public partial class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        internal List<SerializedKeyValuePair<TKey, TValue>> _serializedList = new List<SerializedKeyValuePair<TKey, TValue>>();

#if UNITY_EDITOR
        internal IKeyable LookupTable
        {
            get
            {
                if (_lookupTable == null)
                    _lookupTable = new DictionaryLookupTable<TKey, TValue>(this);
                return _lookupTable;
            }
        }

        private DictionaryLookupTable<TKey, TValue> _lookupTable;
#endif

        public void OnAfterDeserialize()
        {
            Clear();

            foreach (var kvp in _serializedList)
            {
#if UNITY_EDITOR
                if (!ContainsKey(kvp.Key))
                    Add(kvp.Key, kvp.Value);
#else
                Add(kvp.Key, kvp.Value);
#endif
            }

#if UNITY_EDITOR
            LookupTable.RecalculateOccurences();
#else
            // Do not clear as this method might be called multiple times for addressables and/or prefab variants
            // _serializedList.Clear();
#endif
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
                LookupTable.RemoveDuplicates();
#endif
        }

        public void Reserialize()
        {
            _serializedList.Clear();

            foreach (var kvp in this)
            {
                var entry = new SerializedKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
                _serializedList.Add(entry);
            }
        }
    }
}
