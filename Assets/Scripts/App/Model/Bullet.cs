using System;
using UnityEngine;


namespace TandC.RunIfYouWantToLive 
{
    public class Bullet
    {
        private GameObject _selfObject;
        public GameObject SelfObject => _selfObject;
        private OnBehaviourHandler _onBehaviourHandler;
        public event Action<Bullet, GameObject> OnColliderEvent;
        private float _lifeTimer = 5f;
        private Vector2 _targetDirection;
        public float Damage { get; set; }
        private float _speed;
        private Rigidbody2D _rigidbody2D;
        private bool _canStartMove = false;
        private GameObject _spriteBullet;
        public Common.Enumerators.WeaponType BulletType { get; private set; }
        public bool IsLife;

        public int DropChance { get; private set; }
        
        private int bulletLife = 1;
        public Bullet(Transform parent, BulletData data, Vector2 direction, float damage, int dropChance, Vector2 startPosition) 
        {
            _selfObject = MonoBehaviour.Instantiate(data.ButlletObject, parent);
            _selfObject.transform.position = startPosition;
            _targetDirection = direction;
            _onBehaviourHandler = _selfObject.GetComponent<OnBehaviourHandler>();
            _spriteBullet = _selfObject.transform.Find("Model").gameObject;
            _rigidbody2D = _selfObject.GetComponent<Rigidbody2D>();
            _onBehaviourHandler.Trigger2DEntered += EndMove;
            Damage = damage;
            DropChance = dropChance;
            _speed = data.BulletSpeed;
            _spriteBullet.SetActive(false);
            _canStartMove = false;
            _lifeTimer = data.bulletLifeTime;
            IsLife = true;
            BulletType = data.type;
            SetRotation();
            
        }

        private void Move() 
        {
            _selfObject.transform.Translate(Vector2.up * _speed * Time.deltaTime);
        }
    
        public void Update()
        {
            _lifeTimer -= Time.deltaTime;

            if (_lifeTimer <= 0)
            {
                IsLife = false;
            }
            if (!_canStartMove)
            {
                return;
            }

            Move();
        }
        private void EndMove(GameObject collider) 
        {
            if (IsLife) 
            {
                if (collider.tag == "Enemy")
                {
                    OnColliderEvent?.Invoke(this, collider);
                }
            }
        }

        public void Dispose() 
        {
            IsLife = false;
            _canStartMove = false;
            if(_selfObject != null) 
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
        private void SetRotation()
        {
            Vector2 direction = _targetDirection - _rigidbody2D.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            _selfObject.transform.localEulerAngles = new Vector3(0, 0, angle);
            //_rigidbody2D.position = _targetDirection;
            _rigidbody2D.freezeRotation = true;
            _spriteBullet.SetActive(true);
            _canStartMove = true;
        }
    }
}

