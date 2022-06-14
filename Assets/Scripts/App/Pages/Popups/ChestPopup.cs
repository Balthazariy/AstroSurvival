using System;
using System.Collections;
using System.Collections.Generic;
using TandC.RunIfYouWantToLive.Common;
using UnityEngine;
using UnityEngine.UI;

namespace TandC.RunIfYouWantToLive
{
    public class ChestPopup : IUIPopup
    {
        public event Action<Enumerators.SkillType> OnSkillChoiceEvent;
        public GameObject Self
        {
            get { return _selfPopup; }
        }

        private ILoadObjectsManager _loadObjectManager;
        private IGameplayManager _gameplayManager;
        private IUIManager _uIManager;
        private ITimerManager _timerManager;

        private GameObject _selfPopup,
                           _container,
                           _skillsContainer,
                           _flashLightContainer,
                           _tapToOpenObject;

        private Button _buttonOk,
                       _buttonTapToOpenChest;

        private Animator _animator;

        private List<Skill> _skills;
        private List<SkillItem> _skillItems;

        public void Init()
        {
            _loadObjectManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _uIManager = GameClient.Get<IUIManager>();
            _timerManager = GameClient.Get<ITimerManager>();    

            _selfPopup = MonoBehaviour.Instantiate(_loadObjectManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ChestPopup"), _uIManager.Canvas.transform);

            _container = _selfPopup.transform.Find("Container").gameObject;
            _skillsContainer = _container.transform.Find("Container_Skills").gameObject;
            _flashLightContainer = _selfPopup.transform.Find("Flashlight").gameObject;
            _tapToOpenObject = _flashLightContainer.transform.Find("Text_OpenChest").gameObject;

            _buttonOk = _selfPopup.transform.Find("Container/Button_Close").GetComponent<Button>();
            _buttonTapToOpenChest = _selfPopup.transform.Find("Flashlight").GetComponent<Button>();

            _animator = _flashLightContainer.GetComponent<Animator>();

            _buttonOk.onClick.AddListener(OkButtonOnClickHandler);
            _buttonTapToOpenChest.onClick.AddListener(TapToOpenChestButtonOnClickHandler);

            _container.SetActive(false);
            _flashLightContainer.SetActive(true);
            _tapToOpenObject.SetActive(true);
            Hide();
        }

        public void Show()
        {
            _selfPopup.SetActive(true);
            _container.SetActive(false);
            _flashLightContainer.SetActive(true);
            _tapToOpenObject.SetActive(true);
            _gameplayManager.PauseGame(true);
            _animator.Play("ChestIdle", -1, 0);
            _buttonTapToOpenChest.interactable = true;

        }

        public void Show(object data)
        {
            Show();
            _skills = (List<Skill>)data;
            _skillItems = new List<SkillItem>();
        }

        public void Hide()
        {
            _gameplayManager.PauseGame(false);
            _selfPopup.SetActive(false);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void SetMainPriority()
        {
        }

        public void FillChestSkills()
        {
            
        }

        #region Button handlers
        private void OkButtonOnClickHandler()
        {
            for (int i = 0; i < _skills.Count; i++)
            {
                OnSkillChoiceEvent?.Invoke(_skills[i].SkillType);
            }
            for (int i = 0; i < _skillItems.Count; i++)
            {
                SkillItem skillItem = _skillItems[i];
                skillItem.selfObject.SetActive(false);
            }
            _skillItems.Clear();
            _skills.Clear();
            Hide();
        }

        private void TapToOpenChestButtonOnClickHandler()
        {
            _animator.Play("ChestOpen", -1, 0);
            _buttonTapToOpenChest.interactable = false;
            _animator.transform.GetComponent<OnBehaviourHandler>().OnAnimationStringEvent += (string value) =>
            {
                for (int i = 0; i < _skills.Count; i++)
                {
                    _skillItems.Add(new SkillItem(_skillsContainer.transform.Find($"Skill_ChestItem_{i}").gameObject, _skills[i].SkillData, false, true));
                    _skillItems[i].selfObject.SetActive(true);
                    _skillItems[i].selfObject.GetComponent<Animator>().Play("ChestItemAnimation", -1, 0);
                }
                _container.SetActive(true);
                _flashLightContainer.SetActive(false);
            };
        }
        #endregion
    }
}