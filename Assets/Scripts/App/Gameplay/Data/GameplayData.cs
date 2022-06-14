using UnityEngine;
using System;
using TandC.RunIfYouWantToLive.Common;

namespace TandC.RunIfYouWantToLive
{
    [CreateAssetMenu(fileName = "GameplayData", menuName = "TandC/Game/GameplayData", order = 3)]
    public class GameplayData : ScriptableObject
    {
        public Enemies[] enemies;
        public WeaponData[] weaponData;
        public PlayerData playerData;
        public BulletData[] bulletData;
        public ItemData[] itemDatas;
        public SkillsData[] skillData;
        public MiniBoss[] miniBosses;
        public Enumerators.SkillType[] StartenSkills;
        public Enumerators.SkillType[] StartenSkills2;
        public DropChanceData DropChance;
        public Phase[] gamePhases;
        public SkillsData GetSkillByType(Enumerators.SkillType skillType)
        {
            foreach (var item in skillData)
            {
                if (item.type == skillType)
                    return item;
            }

            return null;
        }
        public Enemies GetEnemiesByType(Enumerators.EnemyType type) 
        {
            foreach (var item in enemies)
            {
                if (item.type == type)
                    return item;
            }

            return null;
        }
        public MiniBoss GetMiniBossByPhaseId(int phaseId) 
        {
            foreach(var item in miniBosses) 
            {
                foreach(var bossPhases in item.PhaseID) 
                {
                    if(phaseId == bossPhases) 
                    {
                        return item;
                    }
                }
            }
            return null;
        }
        public Phase GetPhaseById(int id) 
        {
            foreach (var item in gamePhases)
            {
                if (item.PhaseId == id)
                    return item;
            }

            return null;
        }
        public WeaponData GetWeaponByType(Enumerators.WeaponType weaponType)
        {
            foreach (var item in weaponData)
            {
                if (item.type == weaponType)
                    return item;
            }

            return null;
        }
        public BulletData GetBulletByType(Enumerators.WeaponType weaponType)
        {
            foreach (var item in bulletData)
            {
                if (item.type == weaponType)
                    return item;
            }

            return null;
        }

        public SkillsData GetSkillByID(int id)
        {
            foreach (var item in skillData)
            {
                if (item.id == id)
                    return item;
            }

            return null;
        }

        public ItemData GetItemDataByType(Enumerators.ItemType itemType) 
        {
            foreach (var item in itemDatas)
            {
                if (item.type == itemType)
                    return item;
            }

            return null;
        }
    }
    [Serializable]
    public class PlayerData
    {
        public int health;
        public int speed;
        public int rotateSpeed;
        public int armor;
        public int startedLevel;
        public int startNeedXp;
        public int startCriticalChance;
        public float criticalDamageMultiplier;
        public float StartDashTimeRecover;
        public int StartDashDamage;
        public float StartMaskRecoverTime;
        public float StartMaskActiveTime;
        public float StartRocketRecoverTime;
        public int StartRocketCount;
        public int StartRocketDamage;
        public int StartHealthCountRestoreByTime;
        public float StartHealthRestoreTime;
        public float StartLaserShotSize;
        public float StartLaserShotTime;
        public int BombDamage;
        public DroneData DroneData;
        public GameObject playerPrefab;
        public Enumerators.WeaponType StartWeaponType;
    }
    [Serializable]
    public class DropChanceData 
    {
        public int StandartShotChance;
        public int LaserShotChance;
        public int RocketBlowChance;
        public int BombBlowChance;
        public int DashChance;
        public int DronChance;
    }
    [Serializable]
    public class DroneData 
    {
        public GameObject prefab;
        public float StartDroneSpeed;
        public int StartDroneDamage;
    }
    [Serializable]
    public class BulletData
    {
        public int BulletSpeed;
        public float bulletLifeTime;
        public GameObject ButlletObject;
        public Enumerators.WeaponType type;
    }

    [Serializable]
    public class MiniBoss
    {
        public Enemies enemyData;
        public int[] PhaseID;
    }
    [Serializable]
    public class Phase 
    {
        public int PhaseId;
      //  public float phaseTime;
        public float spawnTime;
      //  public bool isTimePhase;
        public Enumerators.EnemyType[] enemyInPhase;
        public int[] EnemyCount;
    }

    [Serializable]
    public class Enemies
    {
        public int enemyId;
        public float health;
        public int damage;
        public int experience;
        public float movementSpeed;
        public Vector3 scaleFactor;
        public Enumerators.EnemyMovementType movementType;
        public Enumerators.EnemyType type;
        public GameObject enemyPrefab;
        public Color enemyColor;
        public float lifeTime;
    }
    [Serializable]
    public class ItemData
    {
        public int itemId;
        public int itemValueMin;
        public int itemValueMax;
        public GameObject prefab;
        public Enumerators.ItemType type;
        [TextArea(5, 10)]
        public string description;
    }

    [Serializable]
    public class WeaponData
    {
        public string weaponName;
        public float baseDamage;
        public float shootDeley;
        public Enumerators.WeaponType type;
    }

    [Serializable]
    public class SkillsData
    {
        public uint id;
        public string name;
        public Sprite sprite;
        public int MaxLevel;
        public Skill Skill;
        public float Value = 0;
        public Enumerators.SkillType type;
        public Enumerators.SkillUseType useType;
        [TextArea(5, 10)]
        public string description;
    }
}
