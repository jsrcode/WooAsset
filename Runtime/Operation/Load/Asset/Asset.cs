﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class Asset : AssetHandle
    {

        private Object[] assets;

        private AssetRequest loadOp;
        private float directProgress;
        public Asset(AssetLoadArgs loadArgs) : base(loadArgs)
        {

        }
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (direct)
                    return directProgress;
                if (async)
                {

                    if (loadOp == null)
                        return bundle.progress * 0.25f + dpProgress * 0.5f;
                    return 0.25f + 0.25f * loadOp.progress + dpProgress * 0.5f;
                }
                return bundle.progress * 0.5f + dpProgress * 0.5f;
            }
        }


        protected Object EditorCheck<T>() where T : Object
        {
            if (value is Texture2D)
            {
                if (typeof(T) == typeof(Sprite))
                {
                    var sp = LoadAagain<T>();
                    SetResult(sp);
                }
            }
            return value as T;
        }
        protected virtual Object LoadAagain<T>()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
#else
            return null;
#endif
        }
        public virtual T GetAsset<T>() where T : Object
        {
            if (isDone)
            {
#if UNITY_EDITOR
                EditorCheck<T>();
#endif
                return value as T;
            }
            return null;
        }
        public virtual Type GetAssetType() => isDone && !isErr && !unloaded ? value.GetType() : null;
        public virtual Object[] allAssets => isDone && !isErr && !unloaded ? assets : null;
        public IReadOnlyList<T> GetSubAssets<T>() where T : Object => !isDone || isErr || unloaded
                ? null
                : allAssets
                .Where(x => x is T)
                .Select(x => x as T)
                .ToArray();
        public T GetSubAsset<T>(string name) where T : Object => !isDone || isErr || unloaded
            ? null :
            allAssets
            .Where(x => x.name == name)
            .FirstOrDefault() as T;


        private async void LoadFile()
        {
            if (AssetsHelper.ExistsFile(path))
            {
                RawObject obj = RawObject.Create(path);
                var reader = await AssetsHelper.ReadFile(path, async);
                obj.bytes = reader.bytes;
                SetResult(obj);
            }
            else
            {
                SetErr($"file not exist {path}");
            }
            InvokeComplete();
        }

        protected virtual async void LoadUnityObject()
        {
            await LoadBundle();
            if (bundle.unloaded)
            {
                this.SetErr($"bundle Has been Unloaded  {path}");
                InvokeComplete();
                return;
            }
            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }

            if (assetType == AssetType.Raw)
            {
                var raw = bundle.LoadRawObject(path);
                SetResult(raw);
            }
            else
            {

                if (async)
                {
                    loadOp = bundle.LoadAssetAsync(path, type);
                    await loadOp;
                    assets = loadOp.allAssets;
                    SetResult(loadOp.asset);
                }
                else
                {
                    var result = bundle.LoadAsset(path, type);
                    assets = result;
                    SetResult(result[0]);
                }
            }
        }

        protected sealed override void InternalLoad()
        {

            if (!direct)
                LoadUnityObject();
            else
                LoadFile();
        }

    }

}
