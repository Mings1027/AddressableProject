using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomAddressable
{
    [Serializable]
    public class AddressableSlot<T> where T : UnityEngine.Object
    {
        private AssetReference _current;
        public AssetReference Current => _current;

        public async UniTask<T> LoadAsync(AssetReference next)
        {
            if (_current == next)
            {
                var cached = AddressableManager.Instance.Get<T>(next);
                if (cached != null) return cached;
            }

            Release();
            _current = next;

            if (next == null || !next.RuntimeKeyIsValid())
                return null;
            var result = await AddressableManager.Instance.LoadAsync<T>(next);

            if (_current != next)
            {
                AddressableManager.Instance.Release(next);
                return null;
            }

            return result;
        }

        public T LoadSync(AssetReference next)
        {
            if (_current == next)
            {
                var cached = AddressableManager.Instance.Get<T>(next);
                if (cached != null) return cached;
            }

            Release();
            _current = next;

            if (next == null || !next.RuntimeKeyIsValid())
                return null;

            return AddressableManager.Instance.LoadSync<T>(next);
        }

        public void Release()
        {
            if (_current == null) return;
            AddressableManager.Instance.Release(_current);
            _current = null;
        }
    }
}
