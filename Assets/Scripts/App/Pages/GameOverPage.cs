using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TandC.RunIfYouWantToLive
{
    public class GameOverPage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private INetworkManager _networkManager;
        private IAdvarismetnManager _advarismetnManager;

        private Button _backToMenuButton;
        private Button _recieveButton;

        private bool _isRecieveOneTime;

        private TextMeshProUGUI _scoreValueText;

        private TMP_InputField _nameInputField;
        private int _scoreValue;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _advarismetnManager = GameClient.Get<IAdvarismetnManager>();
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/GameOverPage"), _uiManager.Canvas.transform, false);

            _backToMenuButton = _selfPage.transform.Find("BackToMenu_Button").GetComponent<Button>();
            _recieveButton = _selfPage.transform.Find("Recieve_Button").GetComponent<Button>();
            _scoreValueText = _selfPage.transform.Find("ScoreValue_Text").GetComponent<TextMeshProUGUI>();
            _nameInputField = _selfPage.transform.Find("InputName_Field").GetComponent<TMP_InputField>();
            _backToMenuButton.onClick.AddListener(BackToMenuClickHandler);
            _recieveButton.onClick.AddListener(OnRecieveButtonClickHandler);
            _gameplayManager.GameplayStartedEvent += StartGameInit;
            Hide();
        }

        private void StartGameInit() 
        {
            _isRecieveOneTime = false;
            _recieveButton.interactable = true;
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            _gameplayManager.PauseGame(true);
            _scoreValueText.text = string.Empty;
            _scoreValue = _gameplayManager.GetController<EnemyController>().ScoreCount;
            _scoreValueText.text = _scoreValue.ToString();
        }

        public void Update()
        {

        }

        public void Dispose()
        {

        }

        private void OnRecieveButtonClickHandler() 
        {
            if (_isRecieveOneTime) 
            {
                return;
            }
            _advarismetnManager.ShowAdsVideo(OnRecieveComplete, OnRecieveFailed);


        }
        private void OnRecieveComplete() 
        {
            _gameplayManager.GetController<PlayerController>().RecievePlayer();
            _isRecieveOneTime = false;
            _recieveButton.interactable = false;
            Hide();
            _gameplayManager.PauseGame(false);
            _uiManager.SetPage<GamePage>();
        }
        private void OnRecieveFailed() 
        {
            Debug.LogError("Failed");
        }

        private void RegisterNameInLeaderBoard() 
        {
            if (_nameInputField.text == String.Empty)
            {
                return;
            }
            var recordItem = new RecordItem
            {
                Name = _nameInputField.text,
                Score = _scoreValue,
                EndTime = DateTime.Now.ToString()
            };
            if (_networkManager.IsHasInternetConnection()) 
            {
                _networkManager.StartSend(recordItem.Name, recordItem.Score, recordItem.EndTime);
            }
            _dataManager.AddRecord(recordItem);
            _dataManager.SaveAllCache();
        }

        #region Button handlers
        private void BackToMenuClickHandler()
        {
            Hide();
            RegisterNameInLeaderBoard();
            GameClient.Get<IGameplayManager>().StopGameplay();
            _uiManager.SetPage<StartPage>();
        }

        #endregion
    }
    [Serializable]
    public class RecordItem 
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public string EndTime {get; set; }
    }
}