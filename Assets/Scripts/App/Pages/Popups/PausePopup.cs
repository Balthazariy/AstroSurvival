using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TandC.RunIfYouWantToLive.Common;
using System;

namespace TandC.RunIfYouWantToLive
{
    public class PausePopup : IUIPopup
    {
        public event Action<Enumerators.SkillType> OnSkillChoiceEvent;
        public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectManager;
        private IGameplayManager _gameplayManager;
        private IUIManager _uIManager;

        private GameObject _selfPage;

        private TextMeshProUGUI _scoreText;

        private Button _resumeButton;
        private Button _backToMenuButton;


        public void Init()
        {
            _loadObjectManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _uIManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PausePopup"), _uIManager.Canvas.transform);
            _scoreText = _selfPage.transform.Find("Background_Image/ScoreValue_Text").GetComponent<TextMeshProUGUI>();
            _resumeButton = _selfPage.transform.Find("Background_Image/Resume_Button").GetComponent<Button>();
            _backToMenuButton = _selfPage.transform.Find("Background_Image/BackToMenu_Button").GetComponent<Button>();

            _resumeButton.onClick.AddListener(OnResumeButtonClickHandler);
            _backToMenuButton.onClick.AddListener(OnBackToMenuClickHandler);
            Hide();
        }

        public void Show() 
        {
            _selfPage.gameObject.SetActive(true);
            _gameplayManager.PauseGame(true);
            _scoreText.text = _gameplayManager.GetController<EnemyController>().ScoreCount.ToString();
        }

        public void Show(object data)
        {
            Show();
        }

        public void Hide()
        {
            _selfPage.gameObject.SetActive(false);

        }

        public void Update()
        {
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(Self);
        }

        public void SetMainPriority()
        {
        }

        private void OnResumeButtonClickHandler() 
        {
            _gameplayManager.PauseGame(false);
            Hide();
        }

        private void OnBackToMenuClickHandler() 
        {
            _gameplayManager.StopGameplay();
            _uIManager.SetPage<StartPage>();
            Hide();
        }
    }
}