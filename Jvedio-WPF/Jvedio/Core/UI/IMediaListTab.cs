using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using System;
using System.Collections.Generic;
using static Jvedio.Core.UserControls.VideoItemEventArgs;

namespace Jvedio.Core.UI
{
    /// <summary>媒体列表 Tab 控件契约（当前由 VideoList 实现，Picture/Game 共用 Adapter）。</summary>
    public interface IMediaListTab : ITabItemControl
    {
        string Uid { get; set; }
        TabItemEx TabItemEx { get; }

        event VideoItemEventHandler OnItemClick;
        event VideoItemEventHandler OnItemViewAsso;
        Action<long, float> onGradeChange { get; set; }
        Action<WrapperEventArg<Video>> onRenderSql { get; set; }

        void SetAsso(bool asso);
        void SetActor(ActorInfo actorInfo);
        void RefreshGrade(long dataID, float grade);
        void RefreshTagStamps(long tagID);
        void GenerateScreenShot(List<Video> videos, bool gif = false);
        bool IsShowActor();
        ActorInfo GetCurrentActor();
    }
}
