﻿namespace WooAsset
{
    public class DownLoadBundleOperation : Operation
    {
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return downloader.progress * 0.9f;
            }
        }
        public string bundleName { get; private set; }
        private FileDownloader downloader;

        public DownLoadBundleOperation(string bundleName)
        {
            this.bundleName = bundleName;
            Done();
        }
        private async void Done()
        {
            downloader = AssetsInternal.DownLoadFile(bundleName);
            await downloader;
            if (downloader.isErr)
            {
                SetErr(downloader.error);
            }


            InvokeComplete();
        }

    }

}
