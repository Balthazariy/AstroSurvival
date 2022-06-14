using System;
using UnityEngine;

namespace TandC.RunIfYouWantToLive
{
    public class PlayerObject
    {
        public Action<bool> IsShieldTakeDamageEvent;
        public Action<float, float> HealthUpdateEvent;
        public Action<float> XpUpdateEvent;
        public Action<int> LevelUpdateEvent;
        public event Action OnPlayerDiedEvent;

        private GameObject //_healthBar,
                                _modelObject,
                                _selfObject,
                                _dashSkillContainer;
        private SpriteRenderer _spriteRenderer;
        public GameObject SelfObject => _selfObject;
        public int CurrentLevel { get; private set; }
        private int     _armorAmount,
                        _currentXp,
                        _maxXp;

        private float _currentHealth,
                      _maxHealth,
                      _movementSpeed;

        private Collider2D _collider;

        private Animator _animator;

        private int _rotateSpeed = 1000;

        public Transform SelfTransform { get; private set; }

        private bool _playerGotShieldSkill, 
                        _shieldActive;

        private VariableJoystick _variableJoystick;
        public bool IsAlive;

        public bool IsDashActive;
        private float _dashTimer;
        private bool _isDash;

        public bool IsMaskActive;
        private float _maskTimer;


        public PlayerObject(GameObject selfObject, PlayerData data, VariableJoystick variableJoystick) 
        {
            _selfObject = selfObject;
            SelfTransform = _selfObject.transform;
            _variableJoystick = variableJoystick;
            _variableJoystick.UpdateHandleCenter();
            _modelObject = _selfObject.transform.Find("Model").gameObject;
            _collider  = _selfObject.GetComponent<Collider2D>();
            _dashSkillContainer = _modelObject.transform.Find("[Skills]/DashSkill").gameObject;
            _spriteRenderer = _modelObject.gameObject.GetComponent<SpriteRenderer>();
            _maxHealth = data.health;
            _currentHealth = _maxHealth;
            _movementSpeed = data.speed;
            CurrentLevel = data.startedLevel;
            _rotateSpeed = data.rotateSpeed;
            _armorAmount = data.armor;
            _maxXp = data.startNeedXp;
            _currentXp = 0;
            IsAlive = true;
        }

        public void StartGameEvent() 
        {
            LevelUpdateEvent?.Invoke(CurrentLevel);
            HealthUpdateEvent?.Invoke(_currentHealth, _maxHealth);
            XpUpdateEvent?.Invoke(_currentXp);
        }

        public void SetupShieldSkill(ShieldSkillVFX shieldVFX)
        {
            shieldVFX.IncreaseShieldHealth();
            shieldVFX.ShieldIsActiveEvent += ShieldIsActiveEventHandler;
            IsShieldTakeDamageEvent += (input) =>
            {
                shieldVFX.ShieldTakeDamageEvent?.Invoke();
            };
        }

        public void DecreaseShieldCooldownHandler(ShieldSkillVFX shieldVFX, float value) => shieldVFX.DecreaseShieldRecovery(value);
        public void IncreaseShieldHealthHandler(ShieldSkillVFX shieldVFX) => shieldVFX.IncreaseShieldHealth();

        public void AddXp(int addedXp) 
        {
            _currentXp += addedXp;
            if(_currentXp >= _maxXp) 
            {
                _currentXp -= _maxXp;
                LevelUp();
            }
            XpUpdateEvent?.Invoke(_currentXp);
        }

        public float GetMaxHealthValue() => _maxHealth;
        public float GetMaxExperianceValue() => _maxXp;

        public void TakeDamage(int damage) 
        {
            if (_playerGotShieldSkill && _shieldActive)
            {
                IsShieldTakeDamageEvent?.Invoke(true);
            }
            else
            {
                IsShieldTakeDamageEvent?.Invoke(false);
                PlayerTakeDamage(damage);
            }
        }

        private void PlayerTakeDamage(int damage)
        {
            float tempDamage = damage - ((float)damage / 100 * _armorAmount);
            _currentHealth -= tempDamage;
            HealthUpdateEvent?.Invoke(_currentHealth, _maxHealth);
            if (_currentHealth <= 0)
            {
                PlayerDie();
            }
        }

        public void PlayerRecieve() 
        {
            _modelObject.gameObject.SetActive(true);
            IsAlive = true;
        }

        private void PlayerDie() 
        {
            _modelObject.gameObject.SetActive(false);
            IsAlive = false;
            OnPlayerDiedEvent?.Invoke();
        }

        public void RestoreFullHealth()
        {
            _currentHealth = _maxHealth;
            HealthUpdateEvent?.Invoke(_currentHealth, _maxHealth);
        }
        public void RestoreHealth(int healthValue) 
        {
            _currentHealth += healthValue;
            if(_currentHealth >= _maxHealth) 
            {
                _currentHealth = _maxHealth;
            }
            HealthUpdateEvent?.Invoke(_currentHealth, _maxHealth);
        }

        public void Update()
        {

        }

        public void AddArmor(int amount)
        {
            _armorAmount += amount;
        }

        public void IncreaseMaxHealth(float amount)
        {
            _maxHealth += amount;
            _currentHealth += amount;
            if(_currentHealth > _maxHealth) 
            {
                _currentHealth = _maxHealth;
            }
            HealthUpdateEvent?.Invoke(_currentHealth, _maxHealth);
        }

        public void IncreaseMovementSpeed(float amount)
        {
            _movementSpeed += amount;
        }

        public void GotShieldSkill(bool value)
        {
            _playerGotShieldSkill = value;
        }

        private void ShieldIsActiveEventHandler(bool active)
        {
            _shieldActive = active;
        }  
        
        public void StartAnimationBackToCenter() 
        {
            //Start Animation
        }

        public void OnAnimationBackToCenterEnd() 
        {
            // On Animation end
            SelfTransform.position = new Vector2(0, 0);
        }

        public void FixedUpdate() 
        {
            if (_selfObject == null || !IsAlive) 
            {
                return;
            }
            if (_isDash) 
            {
                Dash();
                //return;
            }
            if (IsMaskActive) 
            {
                _maskTimer -= Time.deltaTime;
                if (_maskTimer <= 0) 
                {
                    EndMask();
                }
            }
            Vector2 movementDirection;
            movementDirection = new Vector2(_variableJoystick.Horizontal, _variableJoystick.Vertical);
            float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);
            movementDirection.Normalize();
            _selfObject.transform.Translate(movementDirection * _movementSpeed * inputMagnitude * Time.deltaTime, Space.World);
            if (_variableJoystick.Vertical != 0 && _variableJoystick.Horizontal != 0)
            {
                Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, movementDirection);
                _selfObject.transform.rotation = Quaternion.RotateTowards(_selfObject.transform.rotation, toRotation, _rotateSpeed * Time.deltaTime);
            }
        }

        private void Dash() 
        {
            if (IsDashActive) 
            {
                _selfObject.transform.Translate(Vector2.up * _movementSpeed * 10 * Time.deltaTime);
            }
            _dashTimer -= Time.deltaTime;
            if(_dashTimer < 0) 
            {
                IsDashActive = false;
                _variableJoystick.enabled = true;
                if(_dashTimer <= -0.5f) 
                {
                    _isDash = false;
                    _dashSkillContainer.SetActive(false);
                }
            }
        }

        public void StartDash(float dashTimer = 0.25f) 
        {
            _dashTimer = dashTimer;
            IsDashActive=true;
            _variableJoystick.UpdateHandleCenter();
            _variableJoystick.enabled = false;
            _dashSkillContainer.SetActive(true);
            _isDash = true;
        }

        public void StartMask(float maskTimer = 3f) 
        {
            IsMaskActive = true;
            _collider.enabled = false;
            _maskTimer = maskTimer;
            _spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
        }
        private void EndMask() 
        {
            _collider.enabled = true;
            _spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            IsMaskActive = false;
        }

        private void LevelUp() 
        {
            _maxXp = (int)(_maxXp * 1.5f);
            CurrentLevel++;
            LevelUpdateEvent?.Invoke(CurrentLevel);
        }

        public void Dispose() 
        {
            MonoBehaviour.Destroy(_selfObject);
        }
    }
}