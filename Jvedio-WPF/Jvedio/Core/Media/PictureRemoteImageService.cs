using Jvedio.Core.Crawler;
using Jvedio.Core.Global;
using Jvedio.Core.Net;
using SuperUtils.IO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Media
{
    /// <summary>网络图片（HTTP/插件 URL）下载到本地缓存，供 NAS/极空间远程资源预览。</summary>
    public static class PictureRemoteImageService
    {
        public static string CacheDir =>
            Path.Combine(PathManager.CurrentUserFolder, "Cache", "PictureRemote");

        public static async Task<string> DownloadToCacheAsync(string url, string cacheKey = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return null;

            string fileName = cacheKey;
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = Math.Abs(url.GetHashCode()).ToString("x8");
            string ext = Path.GetExtension(uri.AbsolutePath);
            if (string.IsNullOrEmpty(ext) || ext.Length > 6)
                ext = ".jpg";

            string dir = CacheDir;
            FileHelper.TryCreateDir(dir);
            string target = Path.Combine(dir, fileName + ext);
            if (File.Exists(target))
                return target;

            var loader = new VideoDownLoader(null, CancellationToken.None, null);
            byte[] bytes = await loader.DownloadImage(url, CrawlerHeader.Default).ConfigureAwait(false);
            if (bytes == null || bytes.Length == 0)
                return null;

            File.WriteAllBytes(target, bytes);
            return target;
        }

        public static bool TryResolveLocalOrRemote(string pathOrUrl, out string localPath)
        {
            localPath = null;
            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return false;
            if (File.Exists(pathOrUrl)) {
                localPath = pathOrUrl;
                return true;
            }
            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out Uri uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) {
                localPath = pathOrUrl;
                return true;
            }
            return false;
        }
    }
}
