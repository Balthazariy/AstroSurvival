using System.Collections.Generic;
using TandC.RunIfYouWantToLive.Helpers;
using UnityEngine;


namespace TandC.RunIfYouWantToLive 
{
    public class DefaultWeapon : Weapon
    {
        private int _doubleShootCount;

        private int _shootAfterShootCount;
        private List<Transform> _useWeapons;
        private Dictionary<int, Transform> _weaponsDirections;
        private List<Transform> _notUseWeapon;
        public DefaultWeapon()
        {
            
        }
        protected override void LocalInit() 
        {
            _notUseWeapon = new List<Transform>();
            _weaponsDirections = new Dictionary<int, Transform>();
            _useWeapons = new List<Transform>();
            for (int i = 0; i < _selfObject.transform.childCount; i++)
            {
                _notUseWeapon.Add(_selfObject.transform.Find($"Gun_{i}"));
            }
            _shootAfterShootCount = 0;
            _doubleShootCount = 0;
        }
        public void UpgradeShootAfterShoot()
        {
            if (_shootAfterShootCount == 4)
            {
                return;
            }
            _shootAfterShootCount++;
        }
        private void ShootAfterShoot()
        {
            for (int i = 0; i <= _shootAfterShootCount; i++)
            {
                GameClient.Instance.GetService<ITimerManager>().AddTimer(Shoot, null, 0.2f * i);
            }
        }

        public void UpgradeDoubleShoot()
        {
            if (_doubleShootCount == 4)
            {
                return;
            }
            _doubleShootCount++;
            RegisterNewWeapon();
        }
        
        protected override void RegisterNewWeapon()
        {
            int weaponIndex = 0;
            if (_useWeapons.Count == 1 || _useWeapons.Count == 3)
            {
                weaponIndex = InternalTools.GetRandomNumberInteger(0, 1);
            }
            var weapon = _notUseWeapon[weaponIndex];
            _useWeapons.Add(weapon);
            _notUseWeapon.Remove(weapon);
            _weaponsDirections.Add(_useWeapons.Count - 1, weapon.Find("ShootDirection"));
        }

        protected override void ShotGetReady()
        {
            if (_shootAfterShootCount > 0)
            {
                ShootAfterShoot();
                return;
            }
            for (int i = 0; i < _useWeapons.Count; i++)
            {
                Shoot();
            }
        }
        protected override void Shoot(object[] data = null)
        {
            for (int i = 0; i < _useWeapons.Count; i++)
            {
                Shoot(_useWeapons[i].position, _weaponsDirections[i].position);
            }
        }


    }
}

