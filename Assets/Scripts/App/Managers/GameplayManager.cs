using System;
using System.Collections.Generic;
using UnityEngine;

namespace TandC.RunIfYouWantToLive
{
    public class GameplayManager : IService, IGameplayManager
    {
        public event Action GameplayStartedEvent;

        public event Action ControllerInitEvent;

        public event Action GameplayEndedEvent;

        private List<IController> _controllers;

        private ILoadObjectsManager _loadObjectsManager;

        public GameObject GameplayObject { get; private set; }

        public Camera GameplayCamera { get; private set; }

        public GameplayData GameplayData { get; private set; }
        public bool IsGameplayStarted { get; private set; }
        public bool IsGamePaused { get; private set; }



        public void Dispose()
        {
            StopGameplay();

            if (_controllers != null)
            {
                foreach (var item in _controllers)
                    item.Dispose();
            }

            // _loadObjectsManager.BundlesDataLoadedEvent -= BundlesDataLoadedEventHandler;
            // _loadObjectsManager.BundlesDataLoadFailedEvent -= BundlesDataLoadFailedEventHandler;
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _controllers = new List<IController>()
            {
                // new NetworkController(),
                // new GameplayController(),
                 new LevelController(),
                 new PlayerController(),
                 new ObjectsController(),
                // new HUDController(),
                 new VFXController(),
                 new EnemyController(),
                 new SkillsController()
            };
            
            foreach (var item in _controllers)
                item.Init();
        }

        public void Update()
        {
            if (!IsGamePaused)
            {
                if (_controllers != null)
                {
                    foreach (var item in _controllers)
                        item.Update();
                }
            }
        }
        public void FixedUpdate()
        {
            if (!IsGamePaused)
            {
                if (_controllers != null)
                {
                    foreach (var item in _controllers)
                        item.FixedUpdate();
                }
            }
        }

        public T GetController<T>() where T : IController
        {
            foreach (var item in _controllers)
            {
                if (item is T)
                {
                    return (T)item;
                }
            }

            throw new Exception("Controller " + typeof(T).ToString() + " have not implemented");
        }

        public void PauseGame(bool enablePause)
        {
            if (enablePause)
            {
                Time.timeScale = 0;
                IsGamePaused = true;
            }
            else
            {
                Time.timeScale = 1;
                IsGamePaused = false;
            }
        }

        public void StartGameplay()
        {
            if (IsGameplayStarted)
                return;
            PauseGame(false);
            GameplayObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Gameplay"));
            GameplayCamera = GameplayObject.transform.Find("GameplayCamera").GetComponent<Camera>();
            GameplayData = _loadObjectsManager.GetObjectByPath<GameplayData>("Data/GameplayData");
            MainApp.Instance.FixedUpdateEvent += FixedUpdate;
            // GetController<GameplayController>().StartGameplay();
            IsGameplayStarted = true;
            ControllerInitEvent?.Invoke();
            GameplayStartedEvent?.Invoke();
        }

        public void StopGameplay()
        {
            if (!IsGameplayStarted)
                return;

            // GetController<GameplayController>().StopGameplay();

            foreach (var item in _controllers)
                item.ResetAll();

            IsGameplayStarted = false;
            MainApp.Instance.FixedUpdateEvent -= FixedUpdate;
            MonoBehaviour.Destroy(GameplayObject);
        }

        public void RestartGameplay()
        {
            StopGameplay();
            StartGameplay();
        }
    }
}