using System;
using System.Collections.Generic;
using TandC.RunIfYouWantToLive.Common;
using UnityEngine;


namespace TandC.RunIfYouWantToLive
{
    public class PlayerController : IController
    {
        private IGameplayManager _gameplayManager;
        private IInputManager _inputManager;
        private IUIManager _uiManager;

        private VFXController _vfxController;

        private GameplayData _gameData;

        private List<Weapon> _allWeapons;

        public PlayerObject Player { get; private set; }

        private ObjectsController _objectsController;
        private EnemyController _enemyController;
        private event Action<object[]> _gameOverEvent;

        public event Action<Enumerators.ActiveButtonType> ActiveButtonEvent;

        private WeaponLine _weaponLine;

        public DroneParent Drones { get; private set; }

        public event Action<float, float> XpUpdateEvent;
        public event Action<float, float> HealthUpdateEvent;
        public event Action<int> LevelUpdateEvent;
        public event Action<int> ScoreUpdateEvent;
        public int CriticalChanceProcent { get; private set; }
        public float CriticalDamageMultiplier { get; private set; }
        public Action<Enumerators.ActiveButtonType,float> SetTimerForButton;
        public Action<int, int> UpdateRocketCount;

        private INetworkManager _networkManager;

        private float _dashRecoverTime;
        public int DashDamage;

        private float _maxRecoverTime;
        public float MaskTime;

        private float _rocketRecoverTime;
        private int _rocketMaxCount;
        private int _rocketCurrentCount;
        public int RocketBlowDamage;

        private float _laserRecoverTime;
        private float _laserShotSize;

        private bool _isRestorePlayerByTime;
        private int _restoreHealthCount;
        private float _restoreHealthTimer;
        private float _restoreHealthTime;

        private float _xpMultiplier;

        public int BombDamage;

        public PlayerController()
        {

        }

        public void Dispose()
        {

        }

        public void IncreaseCriticalChanceProcent(int value) 
        {
            CriticalChanceProcent += value;
        }
        public void IncreaseCriticalDamageMultiplier(float value) 
        {
            CriticalDamageMultiplier += value;
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _inputManager = GameClient.Get<IInputManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _enemyController = _gameplayManager.GetController<EnemyController>();
            _objectsController = _gameplayManager.GetController<ObjectsController>();
            _gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;
            _inputManager.OnDashClickHandler += PlayerDash;
            _inputManager.OnMaskClickHandler += PlayerMask;
            _inputManager.OnRocketShootClickHandler += OnRocketClickHandler;
            _inputManager.OnLaserShootClickHandler += OnLaserClickHandler;
        }

        public void UpgradeWeaponDoubleShot()
        {
            GetWeapon<DefaultWeapon>().UpgradeDoubleShoot();
        }
        public void UpgradeWeaponShotAfterShot()
        {
            GetWeapon<DefaultWeapon>().UpgradeShootAfterShoot();
        }
        public void DecraseDeafaultWeaponShootDeley<T>(float value) where T : Weapon
        {
            GetWeapon<T>().DecreaseShootDeley(value);
        }
        public void IncreaseDefaultBulletSpeed<T>(int value) where T : Weapon
        {
            GetWeapon<T>().IncreaseBulletSpeed(value);
        }
        public void IncreaseBulletDamage<T>(int value) where T : Weapon
        {
            GetWeapon<T>().IncreaseBulletDamage(value);
        }

        public void ActiveButton(Enumerators.ActiveButtonType type) 
        {
            ActiveButtonEvent?.Invoke(type);
        }

        public void UpgradeDronesCount() 
        {
            Drones.RegisterNewDrone();
        }
        public void UpgradeDronesDamage(int value) 
        {
            Drones.UpgradeDamage(value);
        }
        public void UpgradeDroneSpeed(float value) 
        {
            Drones.UpgradeDroneSpeed(value);
        }

        private void GameplayStartedEventHandler()
        {
            _allWeapons = new List<Weapon>();
            Transform parent = _gameplayManager.GameplayObject.transform.Find("[Player]");
            _gameData = _gameplayManager.GameplayData;
            GameObject playerObject = MonoBehaviour.Instantiate(_gameData.playerData.playerPrefab, parent, false);
            Player = new PlayerObject(playerObject, _gameData.playerData, _inputManager.VariableJoystick);
            CriticalChanceProcent = _gameData.playerData.startCriticalChance;
            CriticalDamageMultiplier = _gameData.playerData.criticalDamageMultiplier;
            Player.HealthUpdateEvent += UpdateHealth;
            Player.XpUpdateEvent += UpdateXp;
            Player.LevelUpdateEvent += UpdateLevel;
            Player.OnPlayerDiedEvent += PlayerDieHandler;
            Player.StartGameEvent();
            BombDamage = _gameData.playerData.BombDamage;
            Drones = new DroneParent(MonoBehaviour.Instantiate(_gameData.playerData.DroneData.prefab, parent), _gameData.playerData.DroneData.StartDroneSpeed, _gameData.playerData.DroneData.StartDroneDamage, playerObject.transform);
            Drones.OnDroneHandlerEnter += OnDroneGiveDamageHandler;
            TakeDefaultWeaponWeapons();
            _isRestorePlayerByTime = false;
            _xpMultiplier = 1f;

        }

        private void OnDroneGiveDamageHandler(GameObject enemy, GameObject drone, float damage) 
        {
            _enemyController.HitEnemy(enemy, damage, _gameData.DropChance.DronChance);
        }
        public void ActivateLaserGun() 
        {
            ActiveWeapon(Enumerators.WeaponType.LaserGun);
            _laserShotSize = _gameData.playerData.StartLaserShotSize;
            _laserRecoverTime = _gameData.playerData.StartLaserShotTime;
            ActiveButtonEvent(Enumerators.ActiveButtonType.LaserButton);
            SetLaserShotSize(_laserShotSize);
        }
        public void UpgradeLaserGunSize(float value) 
        {
            _laserShotSize += value;
            SetLaserShotSize(_laserShotSize);
        }
        public void DecreaseLaserShotTime(float value) 
        {
            _laserRecoverTime -= value;
        }
        private void SetLaserShotSize(float size) 
        {
            GameObject laserShotObject = _gameData.GetBulletByType(Enumerators.WeaponType.LaserGun).ButlletObject;
            laserShotObject.transform.localScale = new Vector2(size, laserShotObject.transform.localScale.y);
        }
        public void IncreseXpMultiplier(float value) 
        {
            _xpMultiplier += value;
        }

        private void GameOverHandler(object data = null)
        {
            GameClient.Instance.GetService<ITimerManager>().StopTimer(_gameOverEvent);
            _gameOverEvent -= GameOverHandler;
            _uiManager.SetPage<GameOverPage>();
        }

        private void PlayerDieHandler()
        {
            Drones.HideAll();
            Player.OnPlayerDiedEvent -= PlayerDieHandler;
            _vfxController.SpawnDeathParticles(Player.SelfTransform.position);
            _gameOverEvent += GameOverHandler;
            GameClient.Instance.GetService<ITimerManager>().AddTimer(_gameOverEvent, null, 2f);
        }

        private void UpdateHealth(float health, float maxHealth)
        {
            HealthUpdateEvent?.Invoke(health, Player.GetMaxHealthValue());
        }

        private void UpdateLevel(int level)
        {
            LevelUpdateEvent?.Invoke(level);
        }

        private void UpdateXp(float xpCount)
        {
            XpUpdateEvent?.Invoke(xpCount, Player.GetMaxExperianceValue());
        }

        public void AddXpToPlayer(int xpCount)
        {
            Player.AddXp((int)(xpCount * _xpMultiplier));
        }

        public void RestoreHelathPlayer(int amount)
        {
            Player.RestoreHealth(amount);
        }
        public void FullRestorePlayerHealth()
        {
            Player.RestoreFullHealth();
        }

        public void ActiveMaskSkill() 
        {
            ActiveButtonEvent?.Invoke(Enumerators.ActiveButtonType.MaskButton);
            _maxRecoverTime = _gameData.playerData.StartMaskRecoverTime;
            MaskTime = _gameData.playerData.StartMaskActiveTime;
        }

        public void MaskTimeActiveIncrease(float value) 
        {
            MaskTime += value;
        }

        public void MaskRecoverTimeDecrease(float value) 
        {
            _maxRecoverTime -= value;
        }

        public void PlayerMask() 
        {
            SetTimerForButton?.Invoke(Enumerators.ActiveButtonType.MaskButton, _maxRecoverTime);
            Player.StartMask(MaskTime);
        }

        public void UpgradeRocketMaxCount(int value) 
        {
            _rocketMaxCount += value;
            UpdateRocketCount?.Invoke(_rocketCurrentCount, _rocketMaxCount);
        }

        public bool OnGetRocketBox() 
        {
            if(_rocketCurrentCount == _rocketMaxCount) 
            {
                return false;
            }
            _rocketCurrentCount++;
            UpdateRocketCount?.Invoke(_rocketCurrentCount, _rocketMaxCount);
            return true;
        }
        
        public void BackPlayerToCenter(int value) 
        {
            Player.TakeDamage(value);
            Player.OnAnimationBackToCenterEnd();
        }

        public void BackPlayerToCenterStart()
        {
            Player.StartAnimationBackToCenter();
        }

        private void ShootHandler(Vector2 shotPosition, Vector2 direction, float damage, int dropChance, BulletData bulletData)
        {
            _objectsController.WeaponShoot(bulletData, shotPosition, direction, damage, dropChance);
        }
        public void GetDash()
        {
            ActiveButtonEvent?.Invoke(Enumerators.ActiveButtonType.DashButton);
            _dashRecoverTime = _gameData.playerData.StartDashTimeRecover ;
            DashDamage = _gameData.playerData.StartDashDamage;
        }
        public void DecreaseDashRecoverTime(float value) 
        {
            _dashRecoverTime -= value;
        }
        private void PlayerDash() 
        {
            SetTimerForButton?.Invoke(Enumerators.ActiveButtonType.DashButton, _dashRecoverTime);
            Player.StartDash();
        }
        public void IncreaseDashDamage(int damage) 
        {
            _dashRecoverTime += damage;
        }

        private void OnRocketClickHandler() 
        {
            if(_rocketCurrentCount <= 0)
                {
                return;
            }
            _rocketCurrentCount--;
            UpdateRocketCount?.Invoke(_rocketCurrentCount, _rocketMaxCount);
            OnClickWeapon(Enumerators.WeaponType.RocketLauncer, Enumerators.ActiveButtonType.RocketButton, _rocketRecoverTime);
        }
        private void OnLaserClickHandler() 
        {
            OnClickWeapon(Enumerators.WeaponType.LaserGun, Enumerators.ActiveButtonType.LaserButton, _laserRecoverTime);
        }
        public void ActivatePlayerRestoreHealth() 
        {
            _isRestorePlayerByTime = true;
            _restoreHealthTime = _gameData.playerData.StartHealthRestoreTime;
            _restoreHealthCount = _gameData.playerData.StartHealthCountRestoreByTime;
            _restoreHealthTimer = _restoreHealthTime;
        }
        public void RestoreHealthByTime() 
        {
            RestoreHelathPlayer(_restoreHealthCount);
            _restoreHealthTimer = _restoreHealthTime;
        }
        public void DecreaseRestoreHelathTimer(float value) 
        {
            _restoreHealthTime -= value;
        }
        public void IncreseRestoreCountHealth(int value) 
        {
            _restoreHealthCount += value;
        }

        private void OnClickWeapon(Enumerators.WeaponType type, Enumerators.ActiveButtonType buttonType, float recoverTime) 
        {
            SetTimerForButton?.Invoke(buttonType, recoverTime);
            GetWeaponByType(type).ClickShoot();
        }
        public void RecievePlayer() 
        {
            RestoreHelathPlayer(100);
            Player.PlayerRecieve();
            Player.OnPlayerDiedEvent += PlayerDieHandler;
            _vfxController.SpawnFrozeBombVFX(Player.SelfObject.transform.position);
            _enemyController.FrozeAllEnemy();
            Player.StartMask();
        }
        public void ActivateRocket() 
        {
            _rocketMaxCount = _gameData.playerData.StartRocketCount;
            _rocketCurrentCount = _gameData.playerData.StartRocketCount;
            _rocketRecoverTime = _gameData.playerData.StartRocketRecoverTime;
            RocketBlowDamage = _gameData.playerData.StartRocketDamage;
            UpdateRocketCount?.Invoke(_rocketCurrentCount, _rocketMaxCount);
            ActiveButtonEvent?.Invoke(Enumerators.ActiveButtonType.RocketButton);
        }
        public void UpgradeRocketBlow(int value) 
        {
            RocketBlowDamage += value;
        }
        public void TakeWeapon(WeaponData data)
        {
            Weapon weapon;
            bool isButtonWeapon = false;
            switch (data.type) 
            {
                case Enumerators.WeaponType.Standart:
                    _weaponLine = new WeaponLine(Player.SelfObject.transform.Find("Model/[Weapon]/ShootLine").gameObject);
                    weapon = new DefaultWeapon();
                    break;
                case Enumerators.WeaponType.RocketLauncer:
                    isButtonWeapon = true;
                    weapon = new RocketWeapon();
                    break;
                case Enumerators.WeaponType.LaserGun:
                    isButtonWeapon = true;
                    weapon = new LaserWeapon();
                    break;
                case Enumerators.WeaponType.AutoGun:
                    weapon = new AutoWeapon();
                    break;
                default:
                    weapon = new DefaultWeapon();
                    break;
            }
            weapon.Init(Player.SelfObject.transform.Find($"Model/[Weapon]/{data.weaponName}").gameObject, data, _gameData.GetBulletByType(data.type), _gameData.DropChance.StandartShotChance, isButtonWeapon);
            _allWeapons.Add(weapon);
        }
        public Weapon GetWeaponByType(Enumerators.WeaponType type) 
        {
            foreach(Weapon weapon in _allWeapons) 
            {
                if(weapon.WeaponType == type) 
                {
                    return weapon;
                }
            }
            return null;
        }
        public T GetWeapon<T>() where T : Weapon
        {
            foreach (var weapon in _allWeapons)
            {
                if (weapon is T)
                {
                    return (T)weapon;
                }
            }

            throw new Exception("Weapon " + typeof(T).ToString() + " have not implemented");
        }
        public void ActiveWeapon(Enumerators.WeaponType type) 
        {
            var weapon =  GetWeaponByType(type);
            if (weapon != null)
            {
                if (weapon != null)
                {
                    weapon.OnShootEventHandler -= ShootHandler;
                    weapon.ActiveWeapon(false);
                }
                weapon.OnShootEventHandler += ShootHandler;
                weapon.ActiveWeapon(true);
            }
            else
            {
                throw new System.Exception();
            }
            weapon.ActiveWeapon();
        }
        private void TakeDefaultWeaponWeapons()
        {
            foreach(var weapon in _gameData.weaponData) 
            {
                TakeWeapon(weapon);
            }
            ActiveWeapon(_gameData.playerData.StartWeaponType);
        }
        public void ResetAll()
        {
            Player.HealthUpdateEvent -= UpdateHealth;
            Player.XpUpdateEvent -= UpdateXp;
            Player.LevelUpdateEvent -= UpdateLevel;
            foreach(var weapon in _allWeapons) 
            {
                weapon.OnShootEventHandler -= ShootHandler;
            }
            _allWeapons.Clear();
            Player = null;
        }

        public VFXBase GetSkillOnPlayer(Enumerators.SkillType type)
        {
            return _vfxController.GetSkillVFXByType(type);
        }

        public void AddArmor(int amount)
        {
            Player.AddArmor(amount);
        }

        public void IncreaseMaxHealth(float amount)
        {
            Player.IncreaseMaxHealth(amount);
        }

        public void IncreaseMovementSpeed(float amount)
        {
            Player.IncreaseMovementSpeed(amount);
        }

        public void PlayerGotShieldSkill(bool value)
        {
            Player.GotShieldSkill(value);
        }

        public void Update()
        {
            if (!_gameplayManager.IsGameplayStarted)
                return;
            if (Player != null)
            {
                _gameplayManager.GameplayCamera.transform.position = Player.SelfObject.transform.position;
                Player.Update();
            }
            if (!_gameplayManager.IsGameplayStarted || !Player.IsAlive)
                return;
            if (_isRestorePlayerByTime) 
            {
                _restoreHealthTimer -= Time.deltaTime;
                if(_restoreHealthTimer <= 0) 
                {
                    RestoreHealthByTime();
                }
            }
            Drones.Update();
            foreach (var weapon in _allWeapons) 
            {
                if (!weapon.IsActive) 
                {
                    continue;
                }
                if (weapon != null)
                {
                    if (Player.IsMaskActive || Player.IsDashActive)
                    {
                        return;
                    }
                    if (weapon.WeaponType == Enumerators.WeaponType.Standart)
                    {
                        if (_weaponLine != null && _weaponLine.IsEnemyOnLine)
                        {
                            weapon.CanShoot();
                        }
                    }
                    else
                    {
                        weapon.CanShoot();
                    }
                }
                weapon.Update();
            }
        }

        public void FixedUpdate()
        {
            if (!_gameplayManager.IsGameplayStarted)
                return;
            if (Player != null)
            {
                Player.FixedUpdate();
            }
        }

    }
}