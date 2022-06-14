using System;
using System.Collections.Generic;
using TandC.RunIfYouWantToLive.Common;
using UnityEngine;

namespace TandC.RunIfYouWantToLive
{
    public abstract class Weapon
    {
        public event Action<Vector2, Vector2, float, int, BulletData> OnShootEventHandler;
        public event Action OnReloadEndHandler;
        public event Action OnReloadStartHandler;
        protected GameObject _selfObject;
        protected float _baseDamage;
        protected float _shootDeley = 0f;
        private float _shootTempDeley;

        protected Transform _shotPlace;
        private bool _canShoot = true;
        public int BulletId;
        private Transform _shootDirection;
        protected Vector2 _direction => _shootDirection.position;
        public bool IsActive { get; private set; }
        public bool IsButtonClick;

        private int _dropChance;

        protected BulletData _bulletData;

        protected Transform _weaponTransform;
        protected Transform _weaponDirection;

        public Enumerators.WeaponType WeaponType { get; private set; }

        public Weapon()
        {

        }
        public void Init(GameObject selfObject, WeaponData weaponData, BulletData bulletData, int dropChance, bool isButtonClick = false) 
        {
            _selfObject = selfObject;
            _shotPlace = selfObject.transform;
            _baseDamage = weaponData.baseDamage;
            _shootDeley = weaponData.shootDeley;
            _bulletData = bulletData;
            WeaponType = weaponData.type;
            IsActive = false;
            _shootTempDeley = 1f;
            _dropChance = dropChance;
            IsButtonClick = isButtonClick;
            LocalInit();
            RegisterNewWeapon();
        }
        protected virtual void LocalInit() 
        {

        }
        public void ActiveWeapon()
        {
            IsActive = true;
        }
        protected abstract void RegisterNewWeapon();

        public void DecreaseShootDeley(float value)
        {
            _shootDeley -= value;

        }
        public void IncreaseBulletSpeed(int value)
        {
            _bulletData.BulletSpeed += value;
        }
        public void IncreaseBulletDamage(int value)
        {
            _baseDamage += value;
        }

        public void Update()
        {
            if (!_canShoot)
            {
                _shootTempDeley -= Time.deltaTime;
                if (_shootTempDeley <= 0)
                {
                    _canShoot = true;
                }
            }
        }

        public void ClickShoot()
        {
            ShotGetReady();
        }

        public void CanShoot()
        {
            if (!_canShoot || IsButtonClick)
            {
                return;
            }
            ShotGetReady();
        }

        protected virtual void ShotGetReady()
        {
            Shoot(_weaponTransform.position, _weaponDirection.position);
        }

        protected virtual void Shoot(Vector2 weaponPosition, Vector2 direction, object[] data = null) 
        {
            OnShootEventHandler?.Invoke(weaponPosition, direction, _baseDamage, _dropChance, _bulletData);
            _shootTempDeley = _shootDeley;
            _canShoot = false;
        }

        protected virtual void Shoot(object[] data = null) 
        {
            Shoot(_weaponTransform.position, _weaponDirection.position);
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_selfObject);
        }

        public void ActiveWeapon(bool value)
        {
            _selfObject.gameObject.SetActive(value);
        }
    }
}

