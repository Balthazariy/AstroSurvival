using System.Collections.Generic;
using TandC.RunIfYouWantToLive.Common;
using UnityEngine;
using static TandC.RunIfYouWantToLive.Common.Enumerators;

namespace TandC.RunIfYouWantToLive
{
    public class SkillsController : IController
    {
        private IGameplayManager _gameplayManager;
        private PlayerController _playerController;
        private VFXController _vfxController;

        public List<Skill> ActiveSkills { get; private set; }
        public List<Skill> PassiveSkills { get; private set; }

        public List<Skill> AllAviableSkills { get; private set; }

        private LevelUpPopup _levelUPPopup;
        private GameplayData _gameplayData;
        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();


            _gameplayManager.GameplayStartedEvent += GamePlayStartedEventHandler;
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var upgradeSkillList = FillUpgradeList();
                if (upgradeSkillList != null)
                {
                    GameClient.Get<IUIManager>().DrawPopup<LevelUpPopup>(upgradeSkillList);
                }
            }
#endif

        }

        private void OnLevelUpgradeHandler(int level)
        {
            if (level <= 1)
            {
                return;
            }
            var upgradeSkillList = FillUpgradeList();

            if (upgradeSkillList != null)
            {
                GameClient.Get<IUIManager>().DrawPopup<LevelUpPopup>(upgradeSkillList);
            }

        }

        public List<Skill> FillUpgradeList(int count = Constants.MAX_SKILLS_ON_LEVEL_UPGRADE, bool isForChest = false) 
        {
            List<Skill> tempList = new List<Skill>();
            if (isForChest)
            {
                for (int i = 0; i < AllAviableSkills.Count; i++)
                {
                    if (AllAviableSkills[i].SkillUseType == SkillUseType.Additional)
                    {
                        tempList.Add(AllAviableSkills[i]);
                    }
                }
            }
            else 
            {
                tempList.AddRange(AllAviableSkills);
            }
            var upgradeSkills = new List<Skill>();
            for(int i = 0; i < count; i++) 
            {
                if(tempList.Count <= 0) 
                {
                    break;
                }
                var skill = tempList[UnityEngine.Random.Range(0, tempList.Count)];
                tempList.Remove(skill);
                upgradeSkills.Add(skill);
            }
            if(upgradeSkills.Count <= 0) 
            {
                return null;
            }
            return upgradeSkills;
        }

        public void FixedUpdate()
        {
        }

        private void GetSkillByType(Enumerators.SkillType type)
        {
            foreach (var item in AllAviableSkills)
            {
                if (item.SkillType == type)
                {
                    item.Action();
                    break;
                }
            }
        }

        public Skill GetSkillDataByType(Enumerators.SkillType type)
        {
            foreach (var item in AllAviableSkills)
            {
                if (item.SkillType == type)
                {
                    return item;
                }
            }
            return null;
        }

        private void GamePlayStartedEventHandler()
        {
            _gameplayData = _gameplayManager.GameplayData;
            _playerController = _gameplayManager.GetController<PlayerController>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _levelUPPopup = GameClient.Get<IUIManager>().GetPopup<LevelUpPopup>() as LevelUpPopup;
            var chestPopup = GameClient.Get<IUIManager>().GetPopup<ChestPopup>() as ChestPopup;
            _levelUPPopup.OnSkillChoiceEvent -= GetSkillByType;
            chestPopup.OnSkillChoiceEvent -= GetSkillByType;
            _levelUPPopup.OnSkillChoiceEvent += GetSkillByType;
            chestPopup.OnSkillChoiceEvent += GetSkillByType;
            _gameplayManager.GetController<PlayerController>().LevelUpdateEvent += OnLevelUpgradeHandler;
            InitAllAskills();
        }

        private void InitAllAskills()
        {
            AllAviableSkills = new List<Skill>();
            ActiveSkills = new List<Skill>();
            PassiveSkills = new List<Skill>();
            foreach (var skill in _gameplayData.StartenSkills) 
            {
                AddSkill(skill);
            }
        }

        private void AddSkill(SkillType type) 
        {
            var skill = new Skill(_gameplayData.GetSkillByType(type));
            AllAviableSkills.Add(skill);
            skill.SkillActionEvent += SkillActionHandler;
        }

        private void RemoveSkill(SkillType type) 
        {
            foreach(var skill in AllAviableSkills) 
            {
                if(skill.SkillType == type) 
                {
                    skill.SkillActionEvent -= SkillActionHandler;
                    AllAviableSkills.Remove(skill);
                    break;
                }
            }
        }

        private void SkillActionHandler(Skill skill) 
        {
            switch (skill.SkillType) 
            {
                case SkillType.MaxHealthIncrease:
                    _playerController.IncreaseMaxHealth(skill.Value);
                    break;
                case SkillType.MovementSpeedIncrease:
                    _playerController.IncreaseMovementSpeed(skill.Value);
                    break;
                case SkillType.Shield:
                    _vfxController.PlaySkillVFX(SkillType.Shield);
                    _playerController.Player.SetupShieldSkill(_vfxController.GetSkillVFXByType(SkillType.Shield) as ShieldSkillVFX);
                    RemoveSkill(SkillType.Shield);
                    AddSkill(SkillType.ShieldRecoverTime);
                    AddSkill(SkillType.ShieldHealthIncrease);
                    break;
                case SkillType.Armor:
                    _playerController.AddArmor((int)skill.Value);
                    break;
                case SkillType.ShotAfterShot:
                    _playerController.UpgradeWeaponShotAfterShot();
                    break;
                case SkillType.DoubleShot:
                    _playerController.UpgradeWeaponDoubleShot();
                    break;
                case SkillType.BlowMina:
                    RemoveSkill(SkillType.BlowMina);
                    AddSkill(SkillType.BlowMinaRecover);
                    AddSkill(SkillType.BlowMinaDamage);
                    //Upgrade Start Mina
                    break;
                case SkillType.BlowMinaRecover:
                    //Upgrade Blow Mina Recover
                    break;
                case SkillType.BlowMinaDamage:
                    //Upgrade Blow Mina Damage
                    break;
                case SkillType.ShieldRecoverTime:
                    _playerController.Player.DecreaseShieldCooldownHandler(_vfxController.GetSkillVFXByType(SkillType.Shield) as ShieldSkillVFX, skill.Value);
                    //Upgrade Shield Recover Time
                    break;
                case SkillType.ShieldHealthIncrease:
                    _playerController.Player.IncreaseShieldHealthHandler(_vfxController.GetSkillVFXByType(SkillType.Shield) as ShieldSkillVFX);
                    //Upgrade ShieldHealthIncrease
                    break;
                case SkillType.BulletSpeed:
                    _playerController.IncreaseDefaultBulletSpeed<DefaultWeapon>((int)skill.Value);
                    break;
                case SkillType.ShootDeleyDecrease:
                    _playerController.DecraseDeafaultWeaponShootDeley<DefaultWeapon>(skill.Value);
                    break;
                case SkillType.DamageIncrease:
                    _playerController.IncreaseBulletDamage<DefaultWeapon>((int)skill.Value);
                    break;
                case SkillType.Drone:
                    _playerController.UpgradeDronesCount();
                    if(skill.CurrentLevel <= 1) 
                    {
                        AddSkill(SkillType.IncreaseDroneSpeed);
                        AddSkill(SkillType.IncreaseDroneDamage);
                    }
                    //Upgrade DroneAroundPlayer
                    break;
                case SkillType.IncreaseDroneSpeed:
                    _playerController.UpgradeDroneSpeed(skill.Value);
                    break;
                case SkillType.IncreaseDroneDamage:
                    _playerController.UpgradeDronesDamage((int)skill.Value);
                    break;
                case SkillType.CriticalChanceIncrease:
                    AddSkill(SkillType.CriticalDamageMultilpier);
                    _playerController.IncreaseCriticalDamageMultiplier((int)skill.Value);
                    //Upgrade CriticalChanceIncrease
                    break;
                case SkillType.CriticalDamageMultilpier:
                    _playerController.IncreaseCriticalDamageMultiplier((int)skill.Value);
                    //Upgrade CriticalDamageMultilpierh
                    break;
                case SkillType.AutoGun:
                    AddSkill(SkillType.AutoGunDamageIncrease);
                    AddSkill(SkillType.AutoGunShootDeleyDecrease);
                    RemoveSkill(SkillType.AutoGun);
                  //  _playerController.TakeWeapon(_gameplayData.GetWeaponByType(WeaponType.AutoGun));
                    _playerController.ActiveWeapon(WeaponType.AutoGun);
                    //Upgrade CriticalDamageMultilpierh
                    break;
                case SkillType.AutoGunDamageIncrease:
                    _playerController.IncreaseBulletDamage<AutoWeapon>((int)skill.Value);
                    //Upgrade CriticalDamageMultilpierh
                    break;
                case SkillType.AutoGunShootDeleyDecrease:
                    _playerController.DecraseDeafaultWeaponShootDeley<AutoWeapon>(skill.Value);
                    //Upgrade CriticalDamageMultilpierh
                    break;
                case SkillType.Dash:
                    _playerController.GetDash();
                    AddSkill(SkillType.DashTimeRecoverDecrease);
                    AddSkill(SkillType.DashDamageIncrease);
                    RemoveSkill(SkillType.Dash);
                    break;
                case SkillType.DashDamageIncrease:
                    _playerController.IncreaseDashDamage((int)skill.Value);
                    break;
                case SkillType.DashTimeRecoverDecrease:
                    _playerController.DecreaseDashRecoverTime(skill.Value);
                    break;
                case SkillType.Mask:
                    _playerController.ActiveMaskSkill();
                    AddSkill(SkillType.MaskTimeRecoverDecrease);
                    AddSkill(SkillType.MaskActiveTimeIncrease);
                    RemoveSkill(SkillType.Mask);
                    break;
                case SkillType.MaskTimeRecoverDecrease:
                    _playerController.MaskRecoverTimeDecrease(skill.Value);
                    break;
                case SkillType.MaskActiveTimeIncrease:
                    _playerController.MaskTimeActiveIncrease(skill.Value);
                    break;
                case SkillType.Rocket:
                    _playerController.ActivateRocket();
                    _playerController.ActiveWeapon(WeaponType.RocketLauncer);
                    RemoveSkill(SkillType.Rocket);
                    AddSkill(SkillType.RocketMaxCountUpgrade);
                    AddSkill(SkillType.RocketDamageIncrease);
                    break;

                case SkillType.RocketMaxCountUpgrade:
                    _playerController.UpgradeRocketMaxCount(1);
                    break;
                case SkillType.RocketDamageIncrease:
                    _playerController.UpgradeRocketBlow((int)skill.Value);
                    break;
                case SkillType.HealthRestore:
                    _playerController.ActivatePlayerRestoreHealth();
                    AddSkill(SkillType.HealthRestoreTimeDecrease);
                    AddSkill(SkillType.HealthRestoreCountIncrese);
                    RemoveSkill(SkillType.HealthRestore);
                    break;
                case SkillType.HealthRestoreCountIncrese:
                    _playerController.IncreseRestoreCountHealth((int)skill.Value);
                    break;
                case SkillType.HealthRestoreTimeDecrease:
                    _playerController.DecreaseRestoreHelathTimer(skill.Value);
                    break;
                case SkillType.XpMultiplierIncrese:
                    _playerController.IncreseXpMultiplier(skill.Value);
                    break;
                case SkillType.LaserGun:
                    _playerController.ActivateLaserGun();
                    AddSkill(SkillType.LaserGunSizeIncrese);
                    AddSkill(SkillType.LaserGunTimerDecrease);
                    RemoveSkill(SkillType.LaserGun);
                    break;
                case SkillType.LaserGunSizeIncrese:
                    _playerController.UpgradeLaserGunSize(skill.Value);
                    break;
                case SkillType.LaserGunTimerDecrease:
                    _playerController.DecreaseLaserShotTime(skill.Value);
                    break;
            }
            switch (skill.SkillUseType) 
            {
                case SkillUseType.Active: 
                    ActiveSkills.Add(skill);
                    break;
                case SkillUseType.Passive:
                    PassiveSkills.Add(skill);
                    break;
            }
            if(skill.CurrentLevel == skill.MaxLevel) 
            {
                AllAviableSkills.Remove(skill);
            }
            if (ActiveSkills.Count == 6)
            {
                for(int i = 0; i < AllAviableSkills.Count; i++) 
                {
                    Skill activeSkill = AllAviableSkills[i];
                    if(activeSkill.SkillUseType == SkillUseType.Active) 
                    {
                        AllAviableSkills.Remove(activeSkill);
                    }
                }
            }
            if (PassiveSkills.Count == 6)
            {
                for (int i = 0; i < AllAviableSkills.Count; i++)
                {
                    Skill passive = AllAviableSkills[i];
                    if (passive.SkillUseType == SkillUseType.Passive)
                    {
                        AllAviableSkills.Remove(passive);
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        public void ResetAll()
        {
            _gameplayManager.GetController<PlayerController>().LevelUpdateEvent -= OnLevelUpgradeHandler;
            _levelUPPopup.OnSkillChoiceEvent -= GetSkillByType;
        }
    }
}