using UnityEngine;

namespace TandC.RunIfYouWantToLive 
{
    public class DefaultEnemy : Enemy
    {
        public DefaultEnemy(Transform parent, Enemies data, Transform playerTransform, Vector2 position, bool isBoss = false, bool init = true) : base(parent, data, playerTransform, position, isBoss, init)
        {

        }

        public override void Action()
        {
            _actualCooldown = _attackCooldown;
            IsCanAction = true;
        }

        public override void Move()
        {
            var targetPosition = _playerTransform.position - EnemyTransform.position;
            targetPosition.Normalize();
            _rigidbody2d.MovePosition(EnemyTransform.position + (targetPosition * MovementSpeed * Time.fixedDeltaTime));
        }
    }
}

