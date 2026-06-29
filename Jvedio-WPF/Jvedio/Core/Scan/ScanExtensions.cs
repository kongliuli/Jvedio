using System;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Core.Scan
{
    public static class ScanExtensions
    {
        private const string DEFAULT_VIDEO_EXT = "3g2,3gp,3gp2,3gpp,amr,amv,asf,avi,bdmv,bik,d2v,divx,drc,dsa,dsm,dss,dsv,evo,f4v,flc,fli,flic,flv,hdmov,ifo,ivf,m1v,m2p,m2t,m2ts,m2v,m4b,m4p,m4v,mkv,mp2v,mp4,mp4v,mpe,mpeg,mpg,mpls,mpv2,mpv4,mov,mts,ogm,ogv,pss,pva,qt,ram,ratdvd,rm,rmm,rmvb,roq,rpm,smil,smk,swf,tp,tpr,ts,vob,vp6,webm,wm,wmp,wmv";
        private const string DEFAULT_IMAGE_EXT = "png,jpg,jpeg,bmp,jpe,ico,gif";

        public static string VIDEO_EXTENSIONS { get; private set; }
        public static string PICTURE_EXTENSIONS { get; private set; }
        public static List<string> VIDEO_EXTENSIONS_LIST { get; private set; }
        public static List<string> PICTURE_EXTENSIONS_LIST { get; private set; }
        public static HashSet<string> VIDEO_EXTENSIONS_SET { get; private set; }
        public static HashSet<string> PICTURE_EXTENSIONS_SET { get; private set; }

        static ScanExtensions()
        {
            VIDEO_EXTENSIONS = DEFAULT_VIDEO_EXT;
            PICTURE_EXTENSIONS = DEFAULT_IMAGE_EXT;
            VIDEO_EXTENSIONS_LIST = ToExtensionList(VIDEO_EXTENSIONS);
            PICTURE_EXTENSIONS_LIST = ToExtensionList(PICTURE_EXTENSIONS);
            VIDEO_EXTENSIONS_SET = new HashSet<string>(VIDEO_EXTENSIONS_LIST, StringComparer.OrdinalIgnoreCase);
            PICTURE_EXTENSIONS_SET = new HashSet<string>(PICTURE_EXTENSIONS_LIST, StringComparer.OrdinalIgnoreCase);
        }

        public static List<string> ToExtensionList(string extensions)
        {
            return extensions.Split(',').Select(arg => "." + arg).ToList();
        }
    }
}
