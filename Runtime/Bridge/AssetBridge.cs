﻿namespace WooAsset
{
    public abstract class AssetBridge : IAssetBridge
    {
        private AssetHandle handle;
        protected AssetBridge(AssetHandle handle)
        {
            this.handle = handle;
        }

        protected abstract bool CouldRelease();
        bool IAssetBridge.CouldRelease()
        {
            return CouldRelease();
        }
        void IAssetBridge.Release()
        {
            Assets.Release(handle);
        }
    }
    public abstract class AssetBridge<T> : AssetBridge where T : class
    {
        public T context;
        protected AssetBridge(T context, AssetHandle handle) : base(handle)
        {
            this.context = context;
        }
    }
}
