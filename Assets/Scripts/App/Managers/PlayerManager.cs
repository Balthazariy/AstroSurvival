using Newtonsoft.Json;
using System;
using TandC.RunIfYouWantToLive.Common;
using UnityEngine;
using System.Collections.Generic;

namespace TandC.RunIfYouWantToLive
{
    public class PlayerManager : IService, IPlayerManager
    {
        private INetworkManager _networkManager;
        private IDataManager _dataManager;


      //  public Player LocalPlayer { get; set; }


        public void Dispose()
        {
        }

        public void Init()
        {
            _networkManager = GameClient.Get<INetworkManager>();
            _dataManager = GameClient.Get<IDataManager>();

          //  LocalPlayer = new Player();
        }

        public void Update()
        {
        }
    }
}