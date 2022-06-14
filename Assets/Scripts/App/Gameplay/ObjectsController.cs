using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using TandC.RunIfYouWantToLive.Helpers;
using UnityEngine;
using static TandC.RunIfYouWantToLive.Common.Enumerators;

namespace TandC.RunIfYouWantToLive
{
    public class ObjectsController : IController
    {
        private IGameplayManager _gameplayManager;
        private IUIManager _UIManager;
        public Action<bool> OnPlayerInBorderHandler;
        private VFXController _vfxController;
        private PlayerController _playerController;
        private EnemyController _enemyController;
        private SkillsController _skillsController;
        private GameplayData _gameplayData;
        private Transform _bulletContainer;
        private Transform _itemContainer;
        private List<Bullet> _bulletList = new List<Bullet>();
        private List<Bullet> _invokedBulletsList = new List<Bullet>();
        private List<Item> _items;
        //private GamePage _gamePage;

        public List<Bullet> InvokedBulletsList
        {
            get { return _invokedBulletsList; }
        }
        private const float _playerLeaveBorderTime = 5f;
        private float _playerLeaveBorderTimer;
        private bool _isPlayerInBorder;
        private GameObject _starsParticle;
        public GameObject LaserShot;
        private float _laserGunDamage;
        private Blow _rocketBlow;
        private Blow _bombBlow;
        //private ChestMarker _chestMarker;

        public ObjectsController()
        {

        }

        public void Dispose()
        {
          
        }
        private void BulletInvoked(Bullet bullet, GameObject collider) 
        {
            bool takeShot = _enemyController.HitEnemy(collider, bullet.Damage, bullet.DropChance);
            if (takeShot) 
            {
               
                if (bullet.BulletType == WeaponType.RocketLauncer) 
                {
                    var _rocketBlow = new Blow(_vfxController.SpawnRocketBlow(bullet.SelfObject.transform.position), _playerController.RocketBlowDamage, _gameplayData.DropChance.RocketBlowChance);
                    _rocketBlow.OnGetEnemy += HitBlowEnemy;
                }
                else 
                {
                    _vfxController.SpawnHitParticles(bullet.SelfObject.transform.position, bullet.SelfObject.transform.eulerAngles);
                }
                if (bullet.BulletType == WeaponType.LaserGun) 
                {
                    return;
                }
                bullet.Dispose();
            }
        }
        private void HitBlowEnemy(GameObject collider, float damage, int dropChance) 
        {
            _enemyController.HitEnemy(collider, damage, dropChance);   
        }
        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _UIManager = GameClient.Get<IUIManager>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;
        }

        private void GameplayStartedEventHandler()
        {
            Transform parent = _gameplayManager.GameplayObject.transform.Find("[Objects]");
            _playerController = _gameplayManager.GetController<PlayerController>();
            _enemyController = _gameplayManager.GetController<EnemyController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _bulletContainer = parent.Find("[Bullets]");
            _itemContainer = parent.Find("[Items]");
            _items = new List<Item>();
            _bulletList = new List<Bullet>();
            _invokedBulletsList = new List<Bullet>();
            _gameplayData = _gameplayManager.GameplayData;
            _isPlayerInBorder = true;
            var border = new Border(parent.transform.Find("Border").gameObject, _playerController.Player.SelfObject);
            border.PlayerBackGameLocationEvent += BackToBorderHandler;
            border.PlayerLeaveBorderEvent += PlayerLeaveBorderHandler;
            _starsParticle = parent.transform.Find("Particle_Star").gameObject;
            //_gamePage = GameClient.Get<IUIManager>().GetPage<GamePage>() as GamePage;

        }
        private void OnItemDestory(Item item) 
        {
            switch (item.ItemType)
            {
                case ItemType.SmallXp:
                case ItemType.BigXp:
                case ItemType.MeduimXp:
                    _playerController.AddXpToPlayer(item.ItemValue);
                    break;
                case ItemType.Medecine:
                    _playerController.RestoreHelathPlayer(item.ItemValue);
                    break;
                case ItemType.FrozenBomb:
                    _vfxController.SpawnFrozeBombVFX(item.SelfObject.transform.position);
                    _enemyController.FrozeAllEnemy();
                    break;
                case ItemType.Bomb:
                    _bombBlow = new Blow(_vfxController.SpawnBombBlow(item.SelfObject.transform.position), _playerController.BombDamage, _gameplayData.DropChance.BombBlowChance);
                    break;
                case ItemType.Chest:
                    GameClient.Get<IUIManager>().DrawPopup<ChestPopup>(_skillsController.FillUpgradeList(item.ItemValue, true));
                    //_chestMarker.isChestActive = false;
                    //_gamePage.SetMarkerActive(false);
                    break;
                case ItemType.RocketBox:
                    if (!_playerController.OnGetRocketBox()) 
                    {
                        return;
                    }
                    break;
            }
            item.Dispose();
            _items.Remove(item);
        }
        //public bool IsLaserShot(GameObject gameObject, out float damage) 
        //{
        //    damage = 0;
        //    if(LaserShot != null) 
        //    {
        //        if(gameObject == LaserShot) 
        //        {
        //            damage = _laserGunDamage;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public void OnEnemyDeath(Enemy enemy) 
        {
            ItemData itemData = GetRandomItem(enemy.DropChance,enemy.IsBoss);
            if(itemData == null) 
            {
                return;
            }
            int itemValue = InternalTools.GetRandomNumberInteger(itemData.itemValueMin, itemData.itemValueMax);
            Item item = new Item(itemData.prefab, _itemContainer, enemy.EnemyTransform.position, itemData.type, itemValue);

            //if (item.ItemType == ItemType.Chest)
            //{
            //    _chestMarker = new ChestMarker(item.SelfObject.transform.position, _UIManager.UICamera, _playerController, _gamePage.GetMarkerObject());
            //    _chestMarker.isChestActive = true;
            //    _gamePage.SetMarkerActive(true);
            //}

            item.ItemDestroyHandler += OnItemDestory;
            _items.Add(item);
        }



        private ItemData GetRandomItem(int dropChance, bool isBoss = false) 
        {
            ItemData itemData;
            if (isBoss) 
            {
                var itemChance = InternalTools.GetRandomNumberInteger(0, 100);
                if (_skillsController.ActiveSkills.Count <= 0 && _skillsController.PassiveSkills.Count <= 0)
                {
                    itemChance = 25;
                }
                if (itemChance <= 50)
                {
                    itemData = _gameplayData.GetItemDataByType(Common.Enumerators.ItemType.BigXp);
                }
                else if (itemChance <= 100)
                {
                    itemData = _gameplayData.GetItemDataByType(Common.Enumerators.ItemType.Chest);

                }
                else 
                {
                    return null;
                }
                return itemData;
            }
            var chance = InternalTools.GetRandomNumberInteger(0, 100);
            if(chance <= dropChance) 
            {

                var itemChance = InternalTools.GetRandomNumberInteger(0, 100);

                if (itemChance <= 5) 
                {
                    itemData = _gameplayData.GetItemDataByType(Common.Enumerators.ItemType.FrozenBomb);
                }
                else if (dropChance <= 10)
                {
                    itemData = _gameplayData.GetItemDataByType(Common.Enumerators.ItemType.Bomb);
                }
                else if(itemChance <= 14) 
                {
                    itemData = _gameplayData.GetItemDataByType(Common.Enumerators.ItemType.Medecine);
                }
                else if (itemChance <= 23 && _playerController.GetWeaponByType(WeaponType.RocketLauncer).IsActive)
                {
                    itemData = _gameplayData.GetItemDataByType(Common.Enumerators.ItemType.RocketBox);
                }
                else if(itemChance <= 100 )
                {
                    itemData = _gameplayData.GetItemDataByType(Common.Enumerators.ItemType.SmallXp);
                }
                else 
                {
                    return null;
                }
                return itemData;

            }
            else 
            {
                return null;
            }
        }

        public void WeaponShoot(BulletData bulletdata, Vector2 shotPosition, Vector2 direction, float damage, int dropChance) 
        {
            if(bulletdata != null) 
            {
                Bullet bullet = new Bullet(_bulletContainer, bulletdata, direction, damage, dropChance, shotPosition);
                bullet.OnColliderEvent += BulletInvoked;
                if (bulletdata.type == Common.Enumerators.WeaponType.LaserGun)
                {
                    _laserGunDamage = damage;
                    LaserShot = bullet.SelfObject;
                }
                //else 
                //{

                //}

                _bulletList.Add(bullet);

            };
        }

        private void PlayerLeaveBorderHandler() 
        {
            if (_playerController.Player.IsMaskActive) 
            {
                return;
            }
            OnPlayerInBorderHandler?.Invoke(true);
            _playerLeaveBorderTimer = _playerLeaveBorderTime;
            _isPlayerInBorder = false;
            _playerController.BackPlayerToCenterStart();
        }
        private void BackToBorderHandler() 
        {
            OnPlayerInBorderHandler?.Invoke(false);
            ResetTimer();
        }
        private void ResetTimer() 
        {
            _isPlayerInBorder = true;
        }

        public void ResetAll()
        {
            _bulletList.Clear();
            _invokedBulletsList.Clear();
        }

        public void Update()
        {
            if (!_gameplayManager.IsGameplayStarted)
                return;
            if (!_isPlayerInBorder) 
            {
                _playerLeaveBorderTimer -= Time.deltaTime;
                if(_playerLeaveBorderTimer <= 0) 
                {
                    ResetTimer();
                    _playerController.BackPlayerToCenter(40);
                }
            }
            _starsParticle.transform.position = _gameplayManager.GameplayCamera.transform.position * -1.75f;
            for(int i = 0; i < _bulletList.Count; i++) 
            {
                Bullet bullet = _bulletList[i];
                if (!bullet.IsLife) 
                {
                    bullet.Dispose();
                    _bulletList.Remove(bullet);
                }
                bullet.Update();
            }
            foreach(var item in _items) 
            {
                item.Update();
            }
            //if (_chestMarker != null)
            //    _chestMarker.Update();
        }

        public void FixedUpdate()
        {
           
        }
    }

}
