﻿
using System;

namespace WooAsset
{
    public abstract class AssetOperation<T> : Operation, IAsset
    {

        public abstract bool async { get; }
        public T value { get; private set; }
        public DateTime time { get; private set; }


        private bool _unload;
        public bool unloaded => _unload;
        private int _ref;
        public int refCount => _ref;

        void IAsset.LoadAsync()
        {
            time = DateTime.Now;
            OnLoad();
        }
        void IAsset.UnLoad()
        {
            if (unloaded) return;
            OnUnLoad();
            _unload = true;
        }

        protected abstract void OnUnLoad();
        protected abstract void OnLoad();
        protected virtual void SetResult(T value)
        {
            this.value = value;
            InvokeComplete();
        }

        void IAsset.Retain()
        {
            _ref++;
        }
        int IAsset.Release()
        {
            _ref--;
            return _ref;
        }
    }
}
