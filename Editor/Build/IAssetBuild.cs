﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.U2D;
using UnityEngine.Video;
using UnityEngine;
using UnityEditor.Animations;
using static WooAsset.AssetsEditorTool;


namespace WooAsset
{
    public abstract class IAssetBuild
    {
        public virtual bool GetIsRecord(string path) => true;

        public virtual List<string> GetAssetTags(string path) => null;
        public virtual string GetVersion(string settingVersion, AssetTaskContext context) => settingVersion;
        protected virtual AssetType CoverAssetType(string path, AssetType type) => type;
        public AssetType GetAssetType(string path)
        {
            var list = AssetsHelper.ToRegularPath(path).Split('/').ToList();
            if (!list.Contains("Assets") || list.Contains("Editor") || list.Contains("Resources")) return AssetType.Ignore;
            AssetType _type = AssetType.None;
            if (AssetsEditorTool.IsDirectory(path))
                _type = AssetType.Directory;
            else
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (path.EndsWith(".meta")) _type = AssetType.Ignore;
                //else if (path.EndsWith(".cs")) _type = AssetType.Ignore;
                else if (path.EndsWith(".prefab")) _type = AssetType.Prefab;
                else if (importer is ModelImporter) _type = AssetType.Model;
                else if (AssetDatabase.LoadAssetAtPath<MonoScript>(path) != null) _type = AssetType.Ignore;
                else if (AssetDatabase.LoadAssetAtPath<LightingDataAsset>(path) != null) _type = AssetType.Ignore;
                else if (AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path) != null) _type = AssetType.Ignore;
                else if (AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path) != null) _type = AssetType.Scene;
                else if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null) _type = AssetType.ScriptObject;
                else if (AssetDatabase.LoadAssetAtPath<Animation>(path) != null) _type = AssetType.Animation;
                else if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null) _type = AssetType.AnimationClip;
                else if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null) _type = AssetType.AnimatorController;
                else if (AssetDatabase.LoadAssetAtPath<Font>(path) != null) _type = AssetType.Font;
                else if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null) _type = AssetType.Mesh;
                else if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) _type = AssetType.Material;
                else if (AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null) _type = AssetType.AudioClip;
                else if (AssetDatabase.LoadAssetAtPath<VideoClip>(path) != null) _type = AssetType.VideoClip;
                else if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null) _type = AssetType.Sprite;
                else if (AssetDatabase.LoadAssetAtPath<Texture>(path) != null) _type = AssetType.Texture;
                else if (AssetDatabase.LoadAssetAtPath<Shader>(path) != null) _type = AssetType.Shader;
                else if (AssetDatabase.LoadAssetAtPath<TextAsset>(path) != null) _type = AssetType.TextAsset;
                else if (AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path) != null) _type = AssetType.ShaderVariant;
                else if (AssetDatabase.LoadAssetAtPath<DefaultAsset>(path) != null) _type = AssetType.Raw;



                _type = CoverAssetType(path, _type);
            }
            return _type;
        }
        public virtual void Create(List<EditorAssetData> assets, List<EditorBundleData> result, EditorPackageData pkg)
        {
            var builds = pkg.builds;
            if (builds == null || builds.Count == 0)
            {
                List<EditorAssetData> Shaders = assets.FindAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                assets.RemoveAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                EditorBundleTool.N2One(Shaders, result);

                List<EditorAssetData> Scenes = assets.FindAll(x => x.type == AssetType.Scene);
                assets.RemoveAll(x => x.type == AssetType.Scene);
                EditorBundleTool.One2One(Scenes, result);
                var tagAssets = assets.FindAll(x => x.tags != null && x.tags.Count != 0);
                assets.RemoveAll(x => tagAssets.Contains(x));
                var tags = tagAssets.SelectMany(x => x.tags).Distinct().ToList();
                tags.Sort();
                foreach (var tag in tags)
                {
                    List<EditorAssetData> find = tagAssets.FindAll(x => x.tags.Contains(tag));
                    tagAssets.RemoveAll(x => find.Contains(x));
                    EditorBundleTool.N2MBySize(find, result);
                }
                List<AssetType> _n2mSize = new List<AssetType>() {
                    AssetType.TextAsset
                };
                List<AssetType> _n2mSizeDir = new List<AssetType>() {
                     AssetType.Texture,
                     AssetType.Material,
                };
                List<AssetType> _one2one = new List<AssetType>() {
                    AssetType.Font,
                    AssetType.AudioClip,
                    AssetType.VideoClip,
                    AssetType.Prefab,
                    AssetType.Model,
                    AssetType.Animation,
                    AssetType.AnimationClip,
                    AssetType.AnimatorController,
                    AssetType.ScriptObject,
                };
                foreach (var item in _one2one)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.One2One(fits, result);
                }
                foreach (var item in _n2mSize)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.N2MBySize(fits, result);
                }
                foreach (var item in _n2mSizeDir)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.N2MBySizeAndDir(fits, result);
                }
                EditorBundleTool.N2MBySizeAndDir(assets, result);
            }
            else
            {
                for (int i = 0; i < builds.Count; i++)
                {
                    var build = builds[i];
                    build.Build(assets, result);
                }
            }
        }

        public virtual IAssetStreamEncrypt GetBundleEncrypt(EditorPackageData pkg, EditorBundleData data, IAssetStreamEncrypt en) => en;
        public virtual int GetEncryptCode(IAssetStreamEncrypt en)
        {
            if (en is NoneAssetStreamEncrypt) return NoneAssetStreamEncrypt.code;
            if (en is DefaultAssetStreamEncrypt) return DefaultAssetStreamEncrypt.code;
            return -1;
        }
        NoneAssetStreamEncrypt none = new NoneAssetStreamEncrypt();
        DefaultAssetStreamEncrypt def = new DefaultAssetStreamEncrypt();
        public virtual IAssetStreamEncrypt GetEncryptByCode(int code)
        {
            if (code == NoneAssetStreamEncrypt.code) return none;
            if (code == DefaultAssetStreamEncrypt.code) return def;
            return null;
        }

    }
}
