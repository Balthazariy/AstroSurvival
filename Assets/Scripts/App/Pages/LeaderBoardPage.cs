using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace TandC.RunIfYouWantToLive
{
    public class LeaderBoardPage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IDataManager _dataManager;
        private INetworkManager _networkManager;

        private Button _backToMenuButton;
        private Button _reloadButton;

        private Toggle _localRecordToggle,
                       _globalRecordToggle;

        private GameObject _localRecordsContent, 
                           _globalRecordsContent,
                           _localRecordsPanel,
                           _globalRecordsPanel,
                           _wrongPanel,
                           _loadingPanel;

        private GameObject _userEntryPrefab;

        private List<UserEntry> _localUserEntry,
                                _globalUserEntry;

        private List<GlobalRecordItem> _globalRecordItems;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _networkManager = GameClient.Get<INetworkManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/LeaderBoardPage"), _uiManager.Canvas.transform, false);
            _userEntryPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/UIObjects/UserEntry");
            _backToMenuButton = _selfPage.transform.Find("BackToMenu_Button").GetComponent<Button>();

            _localRecordToggle = _selfPage.transform.Find("Image_Container/Local_Toggle").GetComponent<Toggle>();
            _globalRecordToggle = _selfPage.transform.Find("Image_Container/Global_Toggle").GetComponent<Toggle>();

            _localRecordsPanel = _selfPage.transform.Find("Panel_LocalRecordScrollContainer").gameObject;
            _globalRecordsPanel = _selfPage.transform.Find("Panel_GlobalRecordScrollContainer").gameObject;

            _localRecordsContent = _localRecordsPanel.transform.Find("ScrollView/Viewport/Content").gameObject;
            _globalRecordsContent = _globalRecordsPanel.transform.Find("ScrollView/Viewport/Content").gameObject;

            _wrongPanel = _selfPage.transform.Find("WrongPanel").gameObject;
            _loadingPanel = _selfPage.transform.Find("LoadingPanel").gameObject;

            _reloadButton = _wrongPanel.transform.Find("Reload_Button").GetComponent<Button>();
            _localUserEntry = new List<UserEntry>();
            _globalUserEntry = new List<UserEntry>();

            _localRecordToggle.onValueChanged.AddListener(OnToggleValueChanged);
            _globalRecordToggle.onValueChanged.AddListener(OnToggleValueChanged);
            _backToMenuButton.onClick.AddListener(BackToMenuClickHandler);
            _reloadButton.onClick.AddListener(OnReloadButtonClickHandler);
            _wrongPanel.gameObject.SetActive(false);
            _loadingPanel.gameObject.SetActive(false);
            Hide();
        }

        private void OnToggleValueChanged(bool isOn) 
        {
            UpdatePanelShow();
        }

        private void UpdatePanelShow() 
        {
            _globalRecordsPanel.SetActive(_globalRecordToggle.isOn);
            _localRecordsPanel.SetActive(_localRecordToggle.isOn);
            if (_globalRecordToggle.isOn) 
            {
                GetGlobalRecords();
            }
            if (_localRecordToggle.isOn) 
            {
                _wrongPanel.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            foreach(var item in _localUserEntry) 
            {
                item.Dispose();
            }
            foreach(var item in _globalUserEntry) 
            {
                item.Dispose();
            }
            _localUserEntry.Clear();
            _globalUserEntry.Clear();
            _selfPage.SetActive(false);
        }

        private void BuildLocalRecords() 
        {
            for(int i = 0; i < _dataManager.UserLocalRecords.Count; i++) 
            {
                var item = _dataManager.UserLocalRecords[i];
                _localUserEntry.Add(new UserEntry(MonoBehaviour.Instantiate(_userEntryPrefab, _localRecordsContent.transform) , i+1, item.Name, item.Score, item.EndTime));
            }
        }

        private void BuildGlobalRecords() 
        {
            _wrongPanel.gameObject.SetActive(false);
            _loadingPanel.SetActive(false);
            for (int i = 0; i < _globalRecordItems.Count; i++)
            {
                var item = _globalRecordItems[i];
                //DateTime time = DateTime.Parse(item.EndTime);
                _globalUserEntry.Add(new UserEntry(MonoBehaviour.Instantiate(_userEntryPrefab, _globalRecordsContent.transform), i + 1, item.Name, item.Score, item.EndTime));
            }
        }
        
        private void OnGetRecords(string json) 
        {
            try
            {
                foreach (var item in _globalUserEntry)
                {
                    item.Dispose();
                }
                _globalUserEntry = new List<UserEntry>();
                _globalRecordItems = JsonConvert.DeserializeObject<List<GlobalRecordItem>>(json);
                BuildGlobalRecords();
            }
            catch (Exception ex)
            {
                OnGetError(json);
            }

        }
        private void OnGetError(string json) 
        {
            _wrongPanel.gameObject.SetActive(true);
            _loadingPanel.SetActive(false);
             Debug.LogError(json);
        }

        private void GetGlobalRecords() 
        {
            _loadingPanel.SetActive(true);
            _networkManager.StartGetData(OnGetRecords, OnGetError);
        }

        public void Show()
        {
            UpdatePanelShow();
            BuildLocalRecords();
            _selfPage.SetActive(true);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }


        #region Button handlers
        private void BackToMenuClickHandler()
        {
            Hide();
            _uiManager.SetPage<StartPage>();
        }

        private void OnReloadButtonClickHandler() 
        {
            GetGlobalRecords();
        }

        #endregion
        [Serializable]
        public class GlobalRecordItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Score { get; set; }
            public string EndTime { get; set; }
        }
        private class UserEntry
        {
            private GameObject _selfObject;

            public UserEntry(GameObject prefab, int number, string name, int score, string endTime) 
            {
                _selfObject = prefab;
                _selfObject.transform.Find("Number/Text").GetComponent<TextMeshProUGUI>().text = number.ToString();
                _selfObject.transform.Find("Name/Text").GetComponent<TextMeshProUGUI>().text = name;
                _selfObject.transform.Find("Score/Text").GetComponent<TextMeshProUGUI>().text = score.ToString();
                _selfObject.transform.Find("Date/Text").GetComponent<TextMeshProUGUI>().text = endTime;
            }

            public void Dispose() 
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
    }
}