using System;
using TandC.RunIfYouWantToLive.Common;
using UnityEngine;

namespace TandC.RunIfYouWantToLive
{
    public class ShieldSkillVFX : VFXBase
    {
        public Action ShieldTakeDamageEvent;
        public Action<bool> ShieldIsActiveEvent;

        private bool _shieldCooldownStart,
                        _shieldActive;

        private float _shieldCooldown = 5f,
                        _shieldCurrentCooldown;
        public int shieldDefaultHealth,
                        shieldCurrentHealth;

        private GameObject _shieldObject;

        private Animator _animator;

        public ShieldSkillVFX(Enumerators.SkillType type) : base(type)
        {
            _playerController.PlayerGotShieldSkill(true);
            _shieldCooldownStart = true;

            _shieldObject = _playerController.Player.SelfObject.transform.Find("Model/[Skills]/ShieldSkill").gameObject;
            _shieldObject.SetActive(false);
            _animator = _shieldObject.GetComponent<Animator>();
            shieldDefaultHealth = 0;

            _shieldCurrentCooldown = 0;

            shieldCurrentHealth = shieldDefaultHealth;

            ShieldTakeDamageEvent += ShieldTakeDamageEventHandler;
        }

        public override void Update()
        {
            base.Update();
            if (_shieldCooldownStart)
            {
                _shieldCurrentCooldown -= Time.deltaTime;
                if (_shieldCurrentCooldown <= 0)
                {
                    _shieldCurrentCooldown = _shieldCooldown;
                    ShieldTurnOn();
                    if(shieldDefaultHealth == shieldCurrentHealth) 
                    {
                        _shieldCooldownStart = false;
                    }
                }
            }
        }

        public void DecreaseShieldRecovery(float value) => _shieldCooldown -= value;
        public void IncreaseShieldHealth() => shieldDefaultHealth++;

        private void ShieldTurnOn()
        {
            shieldCurrentHealth++;
            _animator.Play("Appear", -1, 0);
            _shieldObject.SetActive(true);
            ShieldIsActiveEvent?.Invoke(true);
            _shieldActive = true;
            ShieldDefaultSettings();
        }

        private void ShieldDefaultSettings()
        {
            _shieldCurrentCooldown = _shieldCooldown;
            _shieldCooldownStart = true;
        }

        private void ShieldTakeDamageEventHandler()
        {
            shieldCurrentHealth--;
            _animator.Play("Damage", -1, 0);
            if (_shieldActive)
            {
                if (shieldCurrentHealth <= 0)
                    ShieldTurnOff();
            }
        }

        public void ShieldTurnOff()
        {
            _animator.Play("Break", -1, 0);
            //_shieldObject.SetActive(false);
            _shieldActive = false;
            ShieldIsActiveEvent?.Invoke(false);
            _shieldCooldownStart = true;

        }
    }
}