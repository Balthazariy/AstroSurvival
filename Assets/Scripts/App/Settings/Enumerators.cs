namespace TandC.RunIfYouWantToLive.Common
{
    public class Enumerators
    {
        public enum AppState
        {
            NONE,
            APP_INIT_LOADING,
            MAIN_MENU,
        }

        public enum ButtonState
        {
            ACTIVE,
            DEFAULT
        }

        public enum ItemType 
        {
            SmallXp,
            MeduimXp,
            BigXp,
            Ammo,
            Medecine,
            FrozenBomb,
            RocketBox,
            Chest,
            Bomb,
        }

        public enum ActiveButtonType 
        {
            MaskButton,
            LaserButton,
            RocketButton,
            DashButton,
        }

        public enum SceneType
        {
            MAIN_MENU,
        }

        public enum SoundType : int
        {
            //  CLICK,
            //  OTHER,
            //   BACKGROUND,
        }

        public enum NotificationType
        {
            LOG,
            ERROR,
            WARNING,

            MESSAGE
        }

        public enum Language
        {
            NONE,

            DE,
            EN,
            UK
        }

        public enum ScreenOrientationMode
        {
            PORTRAIT,
            LANDSCAPE
        }

        public enum CacheDataType
        {
            USER_LOCAL_DATA,
            USER_RECORDS_DATA
        }

        public enum NotificationButtonState
        {
            ACTIVE,
            INACTIVE
        }

        public enum EnemyMovementType
        {
            Undefined,
            StandartEnemy,
            MineEnemy,
            ChainSawEnemy,
        }
        public enum EnemyType 
        {
            StandartSquare,
            StandartPentagon,
            Star,
            Mine,
            Saw,
            PiciesFull,
            PiciesHalf,
            PiciesSmall,
            MiniBoss,
            SmallSquare
        }
        public enum WeaponType
        {
            Undefined,
            Standart,
            RocketLauncer,
            AutoGun,
            LaserGun,
        }

        public enum SkillUseType 
        {
            Active,
            Passive,
            Additional,
        }

        public enum SkillType
        {
            MaxHealthIncrease,
            MovementSpeedIncrease,

            Shield,
            ShieldRecoverTime,
            ShieldHealthIncrease,

            Armor,
            ShotAfterShot,
            DoubleShot,

            BlowMina,
            BlowMinaDamage,
            BlowMinaRecover,

            BulletSpeed,
            ShootDeleyDecrease,
            DamageIncrease,

            Drone,
            IncreaseDroneSpeed,
            IncreaseDroneDamage,

            CriticalChanceIncrease,
            CriticalDamageMultilpier,

            AutoGun,
            AutoGunDamageIncrease,
            AutoGunShootDeleyDecrease,

            Dash,
            DashTimeRecoverDecrease,
            DashDamageIncrease,

            Mask,
            MaskTimeRecoverDecrease,
            MaskActiveTimeIncrease,

            Rocket,
            RocketMaxCountUpgrade,
            RocketDamageIncrease,

            HealthRestore,
            HealthRestoreTimeDecrease,
            HealthRestoreCountIncrese,

            XpMultiplierIncrese,

            LaserGun,
            LaserGunSizeIncrese,
            LaserGunTimerDecrease,
        }
    }
}