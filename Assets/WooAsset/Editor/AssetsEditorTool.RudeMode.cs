﻿using System.Collections.Generic;
using System;
using UnityEditor;
using System.Linq;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        private class RudeMode : AssetsMode
        {
            private BundleData data;
            private EditorBundle bundle;
            private AssetTaskParams param;
            private IAssetsBuild assetBuild => param.assetBuild;

            private Dictionary<string, TagAssets> tags = new Dictionary<string, TagAssets>();
            Dictionary<string, string> _fuzzleAssets = new Dictionary<string, string>();





            protected override ManifestData manifest => null;
            protected override bool Initialized() => true;
            protected override Operation CopyToSandBox(string from, string to) => Operation.empty;
            protected override Operation InitAsync(string version, bool again, bool fuzzySearch, Func<VersionData, List<PackageData>> getPkgs)
            {
                param = new AssetTaskParams(TaskPipelineType.EditorSimulate);
                data = new BundleData()
                {
                    length = -1,
                    assets = AssetDatabase.GetAllAssetPaths().Where(x =>
                    {
                        if (!param.GetIsRecord(x))
                            return false;
                        var type = assetBuild.GetAssetType(x);
                        return type != AssetType.Ignore && type != AssetType.Directory;

                    }).ToList(),
                    bundleName = "Rude",
                    enCode = NoneAssetStreamEncrypt.code,
                    dependence = new List<string>(),
                    hash = "Rude",
                    raw = false,
                };
                bundle = new EditorBundle(new BundleLoadArgs()
                {
                    async = false,
                    data = data,
                    dependence = Operation.empty,
                    encrypt = new NoneAssetStreamEncrypt(),
                });
                for (int i = 0; i < data.assets.Count; i++)
                {
                    var assetPath = data.assets[i];
                    if (fuzzySearch)
                    {
                        string assetName_noEx = AssetsHelper.GetFileNameWithoutExtension(assetPath);
                        string dir = AssetsHelper.GetDirectoryName(assetPath);
                        var key = AssetsHelper.ToRegularPath(AssetsHelper.CombinePath(dir, assetName_noEx));
                        if (_fuzzleAssets.ContainsKey(key))
                            AssetsHelper.LogError($"fuzzy search:  same name asset in directory : {dir}  name {assetName_noEx} ");
                        else
                            _fuzzleAssets.Add(key, assetPath);
                    }
                    var _tags = param.GetAssetTags(assetPath);
                    if (_tags == null) continue;
                    for (int j = 0; j < _tags.Count; j++)
                    {
                        var _tag = _tags[j];
                        var tag = AssetsHelper.GetFromDictionary(tags, _tag);
                        if (tag.assets == null) tag.assets = new List<string>();
                        if (tag.assets.Contains(assetPath)) continue;
                        tag.assets.Add(assetPath);
                    }
                }
                return Operation.empty;
            }
            protected override LoadRemoteVersionsOperation LoadRemoteVersions() => new AssetDataBaseCheck();
            protected override Bundle CreateBundle(string bundleName, BundleLoadArgs args) => bundle;

            protected override VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) => new AssetDatabaseCompare(version, pkgs, compareType);
            protected override IReadOnlyList<string> GetAllAssetPaths() => data.assets;
            protected override IReadOnlyList<string> GetAllAssetPaths(string bundleName) => GetAllAssetPaths();
            protected override AssetData GetFuzzyAssetData(string path)
            {
                string target;
                if (_fuzzleAssets.TryGetValue(path, out target))
                    return GetAssetData(target);
                return null;
            }
            protected override AssetData GetAssetData(string assetPath)
            {
                if (!data.assets.Contains(assetPath)) return null;
                return new AssetData()
                {
                    bundleName = data.bundleName,
                    path = assetPath,
                    tags = param.GetAssetTags(assetPath),
                    type = assetBuild.GetAssetType(assetPath),
                };
            }
            protected override IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => data.assets.FindAll(x => x.Contains(name));
            protected override BundleData GetBundleData(string bundleName) => data;
            protected override IReadOnlyList<string> GetAllTags() => tags.Keys.ToArray();
            protected override IReadOnlyList<string> GetTagAssetPaths(string tag)
            {
                var asset = AssetsHelper.GetOrDefaultFromDictionary(tags, tag);
                return asset == null ? null : asset.assets;
            }
        }
    }

}
