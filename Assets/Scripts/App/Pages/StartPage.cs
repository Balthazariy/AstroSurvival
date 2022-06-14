using TandC.RunIfYouWantToLive.Common;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

namespace TandC.RunIfYouWantToLive
{
    public class StartPage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IAdvarismetnManager _advarismetnManager;
        private Button _buttonStart,
                       _buttonUpgrades,
                       _buttonSettings,
                       _buttonLeaderboard;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _advarismetnManager = GameClient.Get<IAdvarismetnManager>();
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/StartPage"), _uiManager.Canvas.transform, false);

            _buttonStart = _selfPage.transform.Find("Container_Button/Button_Start").GetComponent<Button>();
            _buttonUpgrades = _selfPage.transform.Find("Container_Button/Button_Upgrades").GetComponent<Button>();
            _buttonSettings = _selfPage.transform.Find("Container_Button/Button_Settings").GetComponent<Button>();
            _buttonLeaderboard = _selfPage.transform.Find("Container_Button/Button_Leaderboard").GetComponent<Button>();

            _buttonStart.onClick.AddListener(StartButtonOnClickHandler);
            _buttonUpgrades.onClick.AddListener(UpgradesButtonOnClickHandler);
            _buttonSettings.onClick.AddListener(SettingsButtonOnClickHandler);
            _buttonLeaderboard.onClick.AddListener(LeaderboardButtonOnClickHandler);

            Hide();
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Update()
        {

        }

        public void Dispose()
        {

        }

        #region Button handlers
        private void StartButtonOnClickHandler()
        {
            Hide();
            GameClient.Get<IGameplayManager>().StartGameplay();
            _uiManager.SetPage<GamePage>();
        }

        private void UpgradesButtonOnClickHandler()
        {

        }

        private void OnComplete() 
        {
            
        }
        private void OnFail() 
        {
            
        }

        private void SettingsButtonOnClickHandler()
        {
            _advarismetnManager.ShowAdsVideo(OnComplete, OnFail);
        }

        private void LeaderboardButtonOnClickHandler()
        {
            _uiManager.SetPage<LeaderBoardPage>();
        }
        #endregion
    }
}