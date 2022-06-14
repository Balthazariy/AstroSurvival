using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace TandC.RunIfYouWantToLive 
{
    public class AdvarismetnManager : IAdvarismetnManager, IService, IUnityAdsInitializationListener, IUnityAdsListener
    {
        //private static AdvarismetnManager _Instance;
        //public static AdvarismetnManager Instance
        //{
        //    get { return _Instance; }
        //    private set { _Instance = value; }
        //}
        private string _androidGameId = "4784551";
        private string _iosGameId = "4784550";

        private string _video;
        private string _rewardedVideo;
        private bool _testMode = false;

        private Action OnCompleteAds;
        private Action OnFailedAds;

        private string _gameId;
        public void Init()
        {
#if UNITY_ANDROID
            _gameId = _androidGameId;
            _video = "Interstitial_Android";
            _rewardedVideo = "Rewarded_Android";
#elif UNITY_IOS

            _gameId = _iosGameId;
            _video = "Interstitial_iOS";
            _rewardedVideo = "Rewarded_iOS";
#endif
            Advertisement.AddListener(this);
            Advertisement.Initialize(_gameId, _testMode);
        }

        public void ShowAdsVideo(Action CompleteEvent, Action FailedEvent) 
        {
            OnCompleteAds = CompleteEvent;
            OnFailedAds = FailedEvent;
            if (Advertisement.IsReady(_rewardedVideo)) 
            {
                Advertisement.Show(_rewardedVideo);
            }
            else if (Advertisement.IsReady(_video)) 
            {
                Advertisement.Show(_video);
            }
            else 
            {
                OnFailedAds?.Invoke();
            }
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Initialize Ads Complete");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Error Ads Initialize: ({error} - {message})");
        }

        public void OnUnityAdsReady(string placementId)
        {
            Debug.Log($"Ads ready {placementId}");
        }

        public void OnUnityAdsDidError(string message)
        {
            Debug.LogError($"Error - {message}");
        }

        public void OnUnityAdsDidStart(string placementId)
        {
            Debug.Log($"Ads Start {placementId}");
        }

        public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        {
            Debug.Log(showResult);
            if (showResult == ShowResult.Finished || showResult == ShowResult.Skipped)
            {
                OnCompleteAds?.Invoke();
            }
            if (showResult == ShowResult.Failed)
            {
                OnFailedAds?.Invoke();
            }
        }

        public void Update()
        {
          
        }

        public void Dispose()
        {
            
        }
    }
}

