using Jvedio.Entity.Common;
using SuperUtils.Media;
using System;
using System.IO;
using static Jvedio.App;

namespace Jvedio.Entity
{
    public partial class Video
    {        /// <summary>
        /// 获取视频信息 （wmv  10ms，其他  100ms）
        /// </summary>
        public static VideoInfo GetMediaInfo(string videoPath)
        {
            VideoInfo videoInfo = new VideoInfo();
            if (File.Exists(videoPath)) {
                MediaInfo mI = null;
                try {
                    mI = new MediaInfo();
                    mI.Open(videoPath);

                    // 全局
                    string format = mI.Get(StreamKind.General, 0, "Format");
                    string bitrate = mI.Get(StreamKind.General, 0, "BitRate/String");
                    string duration = mI.Get(StreamKind.General, 0, "Duration/String1");
                    string fileSize = mI.Get(StreamKind.General, 0, "FileSize/String");

                    // 视频
                    string vid = mI.Get(StreamKind.Video, 0, "ID");
                    string video = mI.Get(StreamKind.Video, 0, "Format");
                    string vBitRate = mI.Get(StreamKind.Video, 0, "BitRate/String");
                    string vSize = mI.Get(StreamKind.Video, 0, "StreamSize/String");
                    string width = mI.Get(StreamKind.Video, 0, "Width");
                    string height = mI.Get(StreamKind.Video, 0, "Height");
                    string risplayAspectRatio = mI.Get(StreamKind.Video, 0, "DisplayAspectRatio/String");
                    string risplayAspectRatio2 = mI.Get(StreamKind.Video, 0, "DisplayAspectRatio");
                    string frameRate = mI.Get(StreamKind.Video, 0, "FrameRate/String");
                    string bitDepth = mI.Get(StreamKind.Video, 0, "BitDepth/String");
                    string pixelAspectRatio = mI.Get(StreamKind.Video, 0, "PixelAspectRatio");
                    string encodedLibrary = mI.Get(StreamKind.Video, 0, "Encoded_Library");
                    string encodeTime = mI.Get(StreamKind.Video, 0, "Encoded_Date");
                    string codecProfile = mI.Get(StreamKind.Video, 0, "Codec_Profile");
                    string frameCount = mI.Get(StreamKind.Video, 0, "FrameCount");

                    // 音频
                    string aid = mI.Get(StreamKind.Audio, 0, "ID");
                    string audio = mI.Get(StreamKind.Audio, 0, "Format");
                    string aBitRate = mI.Get(StreamKind.Audio, 0, "BitRate/String");
                    string samplingRate = mI.Get(StreamKind.Audio, 0, "SamplingRate/String");
                    string channel = mI.Get(StreamKind.Audio, 0, "Channel(s)");
                    string aSize = mI.Get(StreamKind.Audio, 0, "StreamSize/String");

                    string audioInfo = mI.Get(StreamKind.Audio, 0, "Inform") + mI.Get(StreamKind.Audio, 1, "Inform") + mI.Get(StreamKind.Audio, 2, "Inform") + mI.Get(StreamKind.Audio, 3, "Inform");
                    string vi = mI.Get(StreamKind.Video, 0, "Inform");

                    videoInfo = new VideoInfo() {
                        Format = format,
                        BitRate = vBitRate,
                        Duration = duration,
                        FileSize = fileSize,
                        Width = width,
                        Height = height,

                        DisplayAspectRatio = risplayAspectRatio,
                        FrameRate = frameRate,
                        BitDepth = bitDepth,
                        PixelAspectRatio = pixelAspectRatio,
                        Encoded_Library = encodedLibrary,
                        FrameCount = frameCount,
                        AudioFormat = audio,
                        AudioBitRate = aBitRate,
                        AudioSamplingRate = samplingRate,
                        Channel = channel,
                    };
                } catch (Exception ex) {
                    Logger.Error(ex);
                } finally {
                    mI?.Close();
                }
            }

            if (!string.IsNullOrEmpty(videoInfo.Width) && !string.IsNullOrEmpty(videoInfo.Height))
                videoInfo.Resolution = videoInfo.Width + "x" + videoInfo.Height;
            if (!string.IsNullOrEmpty(videoPath)) {
                videoInfo.Extension = System.IO.Path.GetExtension(videoPath)?.ToUpper().Replace(".", string.Empty);
                videoInfo.FileName = System.IO.Path.GetFileNameWithoutExtension(videoPath);
            }

            return videoInfo;
        }
    }
}

