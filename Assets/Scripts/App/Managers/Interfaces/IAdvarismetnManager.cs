using System;

namespace TandC.RunIfYouWantToLive
{
    public interface IAdvarismetnManager
    {
        void ShowAdsVideo(Action CompleteEvent, Action FailedEvent);
    }
}