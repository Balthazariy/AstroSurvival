using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TandC.RunIfYouWantToLive.Common;

namespace TandC.RunIfYouWantToLive
{
    public class GamePage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IInputManager _inputManager;
        private IGameplayManager _gameplayManager;

        private PlayerController _playerController;

        private Button _rocketShootButton,
                       _laserShootButton,
                       _maskButton,
                       _dashButton,
                       _buttonPause;

        private Image _imageHealthBar,
                      _imageExperianceBar;

        private TextMeshProUGUI _textCurrentLevel,
                                _textExperianceValue,
                                _textHealthValue,
                                _textKillsCount;
        private TextMeshProUGUI _rocketCountText,
                                _backToGameZoneTimerText;
        private Image _dashFillImage,
                      _maskFillImage,
                      _laserFillImage,
                      _rocketFillImage;

        private float _dashTimer;
        private float _maxDashTimer;

        private float _maskTimer;
        private float _maxMaskTimer;

        private float _laserTimer;
        private float _maxLaserTimer;

        private float _rocketTimer;
        private float _maxRocketTimer;

        private GameObject _backToGameZoneContainer;

        private float _backToGameZoneTimer = 5f,
                      _backToGameZoneCurrentTimer;

        private bool _backToGameZoneEnabled;

        private GameObject _markerObject;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _inputManager = GameClient.Get<IInputManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/GamePage"), _uiManager.Canvas.transform, false);

            _backToGameZoneContainer = _selfPage.transform.Find("Container_BackToGameZone").gameObject;
            _backToGameZoneTimerText = _backToGameZoneContainer.transform.Find("Text_Timer").GetComponent<TextMeshProUGUI>();
            _backToGameZoneTimerText.text = "05:00";
            _backToGameZoneContainer.SetActive(false);

            _rocketShootButton = _selfPage.transform.Find("ActiveButtonsPanel/Rocket_Button").GetComponent<Button>();
            _laserShootButton = _selfPage.transform.Find("ActiveButtonsPanel/Laser_Button").GetComponent<Button>();
            _maskButton = _selfPage.transform.Find("ActiveButtonsPanel/Mask_Button").GetComponent<Button>();
            _dashButton = _selfPage.transform.Find("ActiveButtonsPanel/Dash_Button").GetComponent<Button>();
            _buttonPause = _selfPage.transform.Find("Container_TopPanel/Button_Pause").GetComponent<Button>();
            _rocketCountText = _rocketShootButton.transform.Find("Image_CountBG/Text_RocketValue").GetComponent<TextMeshProUGUI>();
            _dashFillImage = _dashButton.transform.Find("RestoreBar_Image").GetComponent<Image>();
            _maskFillImage = _maskButton.transform.Find("RestoreBar_Image").GetComponent<Image>();
            _laserFillImage = _laserShootButton.transform.Find("RestoreBar_Image").GetComponent<Image>();
            _rocketFillImage = _rocketShootButton.transform.Find("RestoreBar_Image").GetComponent<Image>();
            _imageHealthBar = _selfPage.transform.Find("Container_TopPanel/Container_HealthBar/Image_Fillbar").GetComponent<Image>();
            _imageExperianceBar = _selfPage.transform.Find("Container_TopPanel/Container_Experiance/Image_ExperianceFillBar").GetComponent<Image>();

            _markerObject = _selfPage.transform.Find("ChestPointer").gameObject;
            _markerObject.SetActive(false);
            _textCurrentLevel = _selfPage.transform.Find("Container_TopPanel/Container_Experiance/Text_LevelValue").GetComponent<TextMeshProUGUI>();
            _textHealthValue = _selfPage.transform.Find("Container_TopPanel/Container_HealthBar/Text_Value").GetComponent<TextMeshProUGUI>();
            _textExperianceValue = _selfPage.transform.Find("Container_TopPanel/Container_Experiance/Text_Value").GetComponent<TextMeshProUGUI>();
            _textKillsCount = _selfPage.transform.Find("Container_LeftPanel/Container_KIlls/Text_Value").GetComponent<TextMeshProUGUI>();

            GameClient.Get<IInputManager>().VariableJoystick = _selfPage.transform.Find("JoyStick").GetComponent<VariableJoystick>();

            _buttonPause.onClick.AddListener(PauseButtonOnClickHandler);

            _rocketShootButton.onClick.AddListener(OnRocketShootButtonClickHandler);
            _laserShootButton.onClick.AddListener(OnLaserShootClickHandler);
            _maskButton.onClick.AddListener(OnMaskClickHandler);
            _dashButton.onClick.AddListener(OnDashClickHandler);
            _gameplayManager.ControllerInitEvent += InitGameplay;

            ResetBackToGameTimerValue();

            Hide();
        }

        private void InitGameplay()
        {
            _rocketShootButton.gameObject.SetActive(false);
            _laserShootButton.gameObject.SetActive(false);
            _maskButton.gameObject.SetActive(false);
            _dashButton.gameObject.SetActive(false);
            _gameplayManager.GetController<EnemyController>().ScoreUpdateEvent += UpdateScoreText;
            ObjectsController objectsController = _gameplayManager.GetController<ObjectsController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _playerController.HealthUpdateEvent -= UpdateHealthPanel;
            _playerController.XpUpdateEvent -= UpdateExperianceValue;
            _playerController.LevelUpdateEvent -= UpdateLevelText;
            _playerController.ActiveButtonEvent -= ActiveButtonHandler;
            _playerController.SetTimerForButton -= SetTimers;
            objectsController.OnPlayerInBorderHandler -= SetBackToGameObjectContainerActive;
            _gameplayManager.GetController<EnemyController>().ScoreUpdateEvent -= UpdateScoreText;
            _playerController.UpdateRocketCount -= UpdateCurrentRocketCount;
            _playerController.HealthUpdateEvent += UpdateHealthPanel;
            objectsController.OnPlayerInBorderHandler += SetBackToGameObjectContainerActive;
            _playerController.XpUpdateEvent += UpdateExperianceValue;
            _playerController.LevelUpdateEvent += UpdateLevelText;
            _playerController.ActiveButtonEvent += ActiveButtonHandler;
            _playerController.SetTimerForButton += SetTimers;
            _playerController.UpdateRocketCount += UpdateCurrentRocketCount;
            _gameplayManager.GetController<EnemyController>().ScoreUpdateEvent += UpdateScoreText;
        }

        public void SetMarkerActive(bool active) => _markerObject.SetActive(active);
        public GameObject GetMarkerObject() { return _markerObject; }

        private void ActiveButtonHandler(Enumerators.ActiveButtonType type)
        {
            switch (type)
            {
                case Enumerators.ActiveButtonType.DashButton:
                    _dashButton.gameObject.SetActive(true);
                    break;
                case Enumerators.ActiveButtonType.MaskButton:
                    _maskButton.gameObject.SetActive(true);
                    break;
                case Enumerators.ActiveButtonType.RocketButton:
                    _rocketShootButton.gameObject.SetActive(true);
                    break;
                case Enumerators.ActiveButtonType.LaserButton:
                    _laserShootButton.gameObject.SetActive(true);
                    break;

            }

        }

        public void SetBackToGameObjectContainerActive(bool active)
        {
            if (active)
                ResetBackToGameTimerValue();
            _backToGameZoneContainer.SetActive(active);
            _backToGameZoneEnabled = active;
        }
        public void SetBackToGameTimerText(float value) => _backToGameZoneTimerText.text = string.Format(@"{00:00.00}", value).Replace(',', ':');
        public void ResetBackToGameTimerValue() => _backToGameZoneCurrentTimer = _backToGameZoneTimer;

        private void UpdateScoreText(int score)
        {
            _textKillsCount.text = score.ToString();
        }

        private void UpdateLevelText(int level)
        {
            _textCurrentLevel.text = "Level: " + level.ToString();
        }

        private void UpdateHealthPanel(float currentHealth, float maxHealth)
        {
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
            _textHealthValue.text = ((int)currentHealth).ToString() + "/" + maxHealth;
            _imageHealthBar.fillAmount = currentHealth / maxHealth;
        }

        private void UpdateExperianceValue(float value, float neededExperiance)
        {
            _textExperianceValue.text = ((int)(value)).ToString() + "/" + neededExperiance;
            _imageExperianceBar.fillAmount = value / neededExperiance;
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
            if (_dashTimer >= 0)
            {
                _dashTimer -= Time.deltaTime;
                _dashFillImage.fillAmount = _dashTimer / _maxDashTimer;
            }
            if (_maskTimer >= 0)
            {
                _maskTimer -= Time.deltaTime;
                _maskFillImage.fillAmount = _maskTimer / _maxMaskTimer;
            }
            if (_rocketTimer >= 0)
            {
                _rocketTimer -= Time.deltaTime;
                _rocketFillImage.fillAmount = _rocketTimer / _maxRocketTimer;
            }
            if (_laserTimer >= 0)
            {
                _laserTimer -= Time.deltaTime;
                _laserFillImage.fillAmount = _laserTimer / _maxLaserTimer;
            }


            if (_backToGameZoneEnabled)
            {
                _backToGameZoneCurrentTimer -= Time.deltaTime;
                SetBackToGameTimerText(_backToGameZoneCurrentTimer);
                if (_backToGameZoneCurrentTimer <= 0)
                {
                    SetBackToGameTimerText(0f);
                    _backToGameZoneEnabled = false;
                }
            }
        }

        private void UpdateCurrentRocketCount(int rocketCurrentCount, int maxRocketCount) 
        {
            _rocketCountText.text = $"{rocketCurrentCount}/{maxRocketCount}";
        }

        private void SetTimers(Enumerators.ActiveButtonType type, float time) 
        {
            switch (type)
            {
                case Enumerators.ActiveButtonType.DashButton:
                    _dashTimer = time;
                    _maxDashTimer = time;
                    break;
                case Enumerators.ActiveButtonType.MaskButton:
                    _maskTimer = time;
                    _maxMaskTimer = time;
                    float timerForAllButton = _gameplayManager.GetController<PlayerController>().MaskTime;
                    if(_dashTimer <= timerForAllButton) 
                    {
                        SetTimers(Enumerators.ActiveButtonType.DashButton, timerForAllButton);
                    }
                    if (_rocketTimer <= timerForAllButton)
                    {
                        SetTimers(Enumerators.ActiveButtonType.RocketButton, timerForAllButton);
                    }
                    if (_laserTimer <= timerForAllButton)
                    {
                        SetTimers(Enumerators.ActiveButtonType.LaserButton, timerForAllButton);
                    }
                    break;
                case Enumerators.ActiveButtonType.RocketButton:
                    _rocketTimer = time;
                    _maxRocketTimer = time;
                    break;
                case Enumerators.ActiveButtonType.LaserButton:
                    _laserTimer = time;
                    _maxLaserTimer = time;
                    break;

            }
        }

        public void Dispose()
        {

        }

        #region Button Handlers
       
        private void OnRocketShootButtonClickHandler() 
        {
            if (_rocketTimer > 0)
            {
                return;
            }
            _inputManager.OnRocketClick();
        }
        private void OnLaserShootClickHandler() 
        {
            if (_laserTimer > 0)
            {
                return;
            }
            _inputManager.OnLaserClick();
        }
        private void OnDashClickHandler() 
        {
            if(_dashTimer > 0) 
            {
                return;
            }
            _inputManager.OnDashClick();
        }
        private void OnMaskClickHandler() 
        {
            if (_maskTimer > 0)
            {
                return;
            }
            _inputManager.OnMaskClick();
        }
        private void PauseButtonOnClickHandler()
        {
            _uiManager.DrawPopup<PausePopup>();
        }
        #endregion
    }
}