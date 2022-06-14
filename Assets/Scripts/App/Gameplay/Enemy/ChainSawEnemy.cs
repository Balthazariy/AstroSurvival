using UnityEngine;

namespace TandC.RunIfYouWantToLive 
{
    public class ChainSawEnemy : Enemy
    {
        private Transform _model;
        private bool _isInit = false;
        public ChainSawEnemy(Transform parent, Enemies data, Transform playerTransform, Vector2 position, bool isBoss = false, bool init = true) : base(parent, data, playerTransform, position, isBoss, init)
        {
            _model = SelfObject.transform.Find("Model");
            Vector2 direction = playerTransform.position - SelfObject.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            SelfObject.transform.localEulerAngles = new Vector3(0, 0, angle);
            _rigidbody2d.freezeRotation = true;
            _isInit = true;
        }

        public override void Action()
        {
            _actualCooldown = _attackCooldown;
            IsCanAction = true;
        }

        public override void Move()
        {
            if (_isInit) 
            {
                _model.Rotate(0, 0, 300 * Time.deltaTime);
                SelfObject.transform.Translate(Vector2.up * MovementSpeed * 5 * Time.deltaTime);
            }
        }
    }
}

