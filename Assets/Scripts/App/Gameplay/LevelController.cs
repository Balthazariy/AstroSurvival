using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TandC.RunIfYouWantToLive
{
    public class LevelController :IController
    {
        private IGameplayManager _gameplayManager;
        public LevelController()
        {

        }

        public void Dispose()
        {
          
        }

        public void Init()
        {
            //_gameplayManager = GameClient.Get<IGameplayManager>();
            //     _gameplayController.GameConfigUpdatedEvent += GameConfigUpdatedEventHandler;
            //_gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;
        }

        private void GameplayStartedEventHandler()
        {
            //Transform parent = _gameplayManager.GameplayObject.transform.Find("ViewContainer/[Level]");


        }

        public void ResetAll()
        {
            
        }

        public void Update()
        {

        }

        public void FixedUpdate()
        {
           
        }
    }

}
