using System.Collections.Generic;
using TandC.RunIfYouWantToLive.Helpers;
using UnityEngine;


namespace TandC.RunIfYouWantToLive 
{
    public class AutoWeapon : Weapon
    {
        private List<GameObject> _enemies;
        private OnBehaviourHandler _behaviourHandler;

        public AutoWeapon()
        {
           
        }

        private void OnHandlerEnter(GameObject collider) 
        {
            if(collider.tag == "Enemy") 
            {
                _enemies.Add(collider);
            }
        }
        private void OnHandlerExit(GameObject collider) 
        {
            if (collider.tag == "Enemy")
            {
                foreach(var enemy in _enemies) 
                {
                    if(enemy == collider) 
                    {
                        _enemies.Remove(enemy);
                        break;
                    }
                }
            }
        }
        
        protected override void RegisterNewWeapon()
        {
            _enemies = new List<GameObject>();
            _weaponTransform = _selfObject.transform;
            _behaviourHandler = _selfObject.transform.Find("AimRadius").GetComponent<OnBehaviourHandler>();
            _behaviourHandler.Trigger2DEntered += OnHandlerEnter;
            _behaviourHandler.Trigger2DExited += OnHandlerExit;
        }

        private GameObject FindClosetEnemy()
        {
            try 
            {
                GameObject closetEnemy = _enemies[0];

                for (int i = 0; i <= _enemies.Count - 1; i++)
                {
                    float closetEnemyDistance =
                       Vector2.Distance(closetEnemy.transform.position, _weaponTransform.position);
                    var enemyPlayerDistance = Vector2.Distance(_enemies[i].transform.position,
                       _weaponTransform.position);
                    if (enemyPlayerDistance < closetEnemyDistance)
                    {
                        closetEnemy = _enemies[i];
                    }
                }
                return closetEnemy;
            }
            catch (System.Exception) 
            {
                return null;
            }

        }

        protected override void ShotGetReady()
        {
            if(_enemies.Count <= 0) 
            {
                return;
            }
            GameObject closetEnemy = FindClosetEnemy();
            if(closetEnemy == null) 
            {
                return;
            }
            _weaponDirection = closetEnemy.transform;
            Shoot();
        }

        
    }
}

