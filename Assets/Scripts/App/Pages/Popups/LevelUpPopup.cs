using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TandC.RunIfYouWantToLive.Common;
using System;

namespace TandC.RunIfYouWantToLive
{
    public class LevelUpPopup : IUIPopup
    {
        public event Action<Enumerators.SkillType> OnSkillChoiceEvent; 
        public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectManager;
        private IGameplayManager _gameplayManager;
        private IUIManager _uIManager;
        private IAdvarismetnManager _advarismetnManager;
        private GameObject _selfPage;

        private Transform _skillContainer;

        private Button _continueButton,
                        _viewADButton;
        private TextMeshProUGUI _contratulationText;

        public List<SkillItem> _skillItemsList;
        private Enumerators.SkillType _skillType;

        private GameObject _levelUpPrefab;
        private SkillsController _skillsController;
        private PlayerController _playerController;
        private List<Skill> _skills;

        private bool _isReloadOneTime;
        public void Init()
        {
            _loadObjectManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _uIManager = GameClient.Get<IUIManager>();
            _advarismetnManager = GameClient.Get<IAdvarismetnManager>();
            _selfPage = MonoBehaviour.Instantiate(_loadObjectManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LevelUpPopup"), _uIManager.Canvas.transform);
            _levelUpPrefab = _loadObjectManager.GetObjectByPath<GameObject>("Prefabs/UI/UIObjects/Skill_Item");
            _skillContainer = _selfPage.transform.Find("Container/Skills_Container");

            _continueButton = _selfPage.transform.Find("Container/Button_Continue").GetComponent<Button>();
            _viewADButton = _selfPage.transform.Find("Container/Button_ViewAD").GetComponent<Button>();

            _contratulationText = _selfPage.transform.Find("Container/Text_Description").GetComponent<TextMeshProUGUI>();

            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);
            _viewADButton.onClick.AddListener(ViewADButtonOnClickHandler);
            _continueButton.interactable = false;
            _gameplayManager.ControllerInitEvent += InitGameplay;
            Hide();
        }

        private void InitGameplay()
        {
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
        }

        public void Show() {}

        public void Show(object data)
        {
            _isReloadOneTime = false;
            _viewADButton.interactable = true;
            _selfPage.gameObject.SetActive(true);
            _skills = (List<Skill>)data;
            _contratulationText.text = $"Contatulation! You reached level {_playerController.Player.CurrentLevel}";
            FillSkillList();
            _gameplayManager.PauseGame(true);
        }

        public void Hide()
        {
            _selfPage.gameObject.SetActive(false);
            _continueButton.interactable = false;
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

        private void ContinueButtonOnClickHandler()
        {
            Hide();
            OnSkillChoiceEvent?.Invoke(_skillType);
            ResetSkillList();
            _gameplayManager.PauseGame(false);
            _playerController.AddXpToPlayer(0);
        }

        private void OnCompleteAds() 
        {
            Debug.LogError(1);
            ResetSkillList();
            _skills = _gameplayManager.GetController<SkillsController>().FillUpgradeList();
            FillSkillList();

        }
        private void OnFailedAds() 
        {
            Debug.LogError("Fail");
        }
        private void ViewADButtonOnClickHandler()
        {
            if (_isReloadOneTime)
            {
                return;
            }
            _isReloadOneTime = true;
            _viewADButton.interactable = false;
            _advarismetnManager.ShowAdsVideo(OnCompleteAds, OnFailedAds);
        }

        public void FillSkillList()
        {
            _skillItemsList = new List<SkillItem>();

            SkillItem skillItem;

            for(int i = 0; i < _skills.Count; i++) 
            {
               
                Skill skill = _skills[i];
                 skillItem = new SkillItem(MonoBehaviour.Instantiate(_levelUpPrefab, _skillContainer), skill.SkillData, skill.SkillUseType != Enumerators.SkillUseType.Additional);
                 skillItem.ItemSelectionChangedEvent += ItemSelectEventHandler;
                 _skillItemsList.Add(skillItem);
            }
            foreach (var button in _skillItemsList)
            {
                button.Deselect();
            }
        }

        private void ItemSelectEventHandler(Enumerators.SkillType skillType)
        {
            _skillType = skillType;
            _continueButton.interactable = true;
            foreach (var button in _skillItemsList)
            {
                button.Deselect();
            }
        }

        public void ResetSkillList()
        {
            if (_skillItemsList != null)
            {
                foreach (var item in _skillItemsList)
                    item.Dispose();

                _skillItemsList.Clear();
                _skillItemsList = null;
            }
        }
    }

    public class SkillItem
    {
        public event Action<Enumerators.SkillType> ItemSelectionChangedEvent;

        public GameObject selfObject;

        private Button _selectButton;

        public bool isSelect { get; private set; }

        public bool isChestSkill;

        public Enumerators.SkillType skillType;
        public uint skillId;

        public SkillItem(GameObject prefab, SkillsData data, bool isNew, bool isChestSkill = false)
        {
            selfObject = prefab;
            _selectButton = selfObject.GetComponent<Button>();

            selfObject.transform.Find("Image_Icon").GetComponent<Image>().sprite = data.sprite;
            selfObject.transform.Find("Text_SkillDescription").GetComponent<TextMeshProUGUI>().text = data.description;
            selfObject.transform.Find("Text_SkillName").GetComponent<TextMeshProUGUI>().text = data.name;
            selfObject.transform.Find("Text_IsNewSkill").gameObject.SetActive(isNew);
            skillId = data.id;
            skillType = data.type;
            SetSelection(false);
            this.isChestSkill = isChestSkill;
            _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

            if (isChestSkill) _selectButton.interactable = false;
        }

        private void SelectButtonOnClickHandler()
        {
            SetSelection(!isSelect);
        }

        public void SetSelection(bool state)
        {
            if (isSelect == state)
                return;

            ItemSelectionChangedEvent?.Invoke(skillType);
            isSelect = state;
            selfObject.GetComponent<Image>().color = new Color(0.05f, 0.8f, 0.4f, 1f);
        }

        public void Deselect()
        {
            selfObject.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);
            isSelect = false;
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(selfObject);
        }
    }
}