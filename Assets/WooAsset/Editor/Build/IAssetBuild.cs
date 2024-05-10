﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.U2D;
using UnityEngine.Video;
using UnityEngine;
using UnityEditor.Animations;

namespace WooAsset
{
    public abstract class IAssetBuild
    {
        public virtual string GetVersion(string settingVersion, AssetTaskContext context) => settingVersion;

        public virtual void HandleLoopDependence(List<EditorAssetData> err)
        {
            foreach (EditorAssetData data in err)
            {
                if (data.type == AssetType.LightingData) data.dependence.Clear();
            }
        }
        protected virtual AssetType CoverAssetType(string path, AssetType type) => type;
        public AssetType GetAssetType(string path)
        {
            var list = AssetsHelper.ToRegularPath(path).Split('/').ToList();
            if (!list.Contains("Assets") || list.Contains("Editor") || list.Contains("Resources")) return AssetType.Ignore;
            AssetType _type = AssetType.None;
            if (AssetsHelper.IsDirectory(path))
                _type = AssetType.Directory;
            else
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (path.EndsWith(".rfc")) _type = AssetType.RawCopyFile;
                else if (path.EndsWith(".meta")) _type = AssetType.Ignore;
                else if (path.EndsWith(".cs")) _type = AssetType.Ignore;
                else if (path.EndsWith(".prefab")) _type = AssetType.Prefab;
                else if (importer is ModelImporter) _type = AssetType.Model;
                else if (AssetDatabase.LoadAssetAtPath<RawObject>(path) != null) _type = AssetType.RawObject;
                else if (AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path) != null) _type = AssetType.Scene;
                else if (AssetDatabase.LoadAssetAtPath<LightingDataAsset>(path) != null) _type = AssetType.LightingData;
                else if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null) _type = AssetType.ScriptObject;
                else if (AssetDatabase.LoadAssetAtPath<Animation>(path) != null) _type = AssetType.Animation;
                else if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null) _type = AssetType.AnimationClip;
                else if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null) _type = AssetType.AnimatorController;
                else if (AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path) != null) _type = AssetType.Ignore;
                else if (AssetDatabase.LoadAssetAtPath<Font>(path) != null) _type = AssetType.Font;
                else if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) _type = AssetType.Material;
                else if (AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null) _type = AssetType.AudioClip;
                else if (AssetDatabase.LoadAssetAtPath<VideoClip>(path) != null) _type = AssetType.VideoClip;
                else if (AssetDatabase.LoadAssetAtPath<Texture>(path) != null) _type = AssetType.Texture;
                else if (AssetDatabase.LoadAssetAtPath<Shader>(path) != null) _type = AssetType.Shader;
                else if (AssetDatabase.LoadAssetAtPath<TextAsset>(path) != null) _type = AssetType.TextAsset;
                else if (AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path) != null) _type = AssetType.ShaderVariant;
                else if (AssetDatabase.LoadAssetAtPath<DefaultAsset>(path) != null) _type = AssetType.Raw;
                _type = CoverAssetType(path, _type);
            }
            return _type;
        }
        public virtual IReadOnlyList<string> GetTags(EditorAssetData info)
        {
            return AssetsEditorTool.option.GetAssetTags(info.path);
        }
        public abstract void Create(List<EditorAssetData> assets, List<BundleGroup> result);

        public virtual List<AssetTask> GetPipelineEndTasks(AssetTaskContext context) => null;
        public virtual List<AssetTask> GetPipelineStartTasks(AssetTaskContext context) => null;
    }
}
