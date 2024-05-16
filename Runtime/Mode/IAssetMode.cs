﻿using static WooAsset.AssetsVersionCollection.VersionData;
using static WooAsset.AssetsVersionCollection;
using System.Collections.Generic;
using System;

namespace WooAsset
{
    public interface IAssetMode
    {
        ManifestData manifest { get; }
        bool Initialized();
        Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        AssetHandle CreateAsset(AssetLoadArgs arg);
        CheckBundleVersionOperation VersionCheck();
        CopyStreamBundlesOperation CopyToSandBox(string from, string to);
        Bundle CreateBundle(string bundleName, BundleLoadArgs args);
    }
}
