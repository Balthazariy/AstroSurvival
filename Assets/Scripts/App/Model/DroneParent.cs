using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TandC.RunIfYouWantToLive
{
    public class DroneParent
    {
        private bool _isActive;
        private GameObject _selfObject;
        private List<Drone> _drons;
        private float _speed;
        public int DronsDamage;
        private int _dronsActiveCount;
        public bool IsColliderOnEnemy;
        private Transform _player;
        public Action<GameObject, GameObject, float> OnDroneHandlerEnter;

        public DroneParent(GameObject prefab, float speed, int damage, Transform player)
        {
            _selfObject = prefab;
            _speed = speed;
            _player = player;
            DronsDamage = damage;
            _drons = new List<Drone>();
            for (int i = 0; i < 4; i++)
            {
                _drons.Add(new Drone(_selfObject.transform.Find($"Drone_{i}").gameObject));
            }
            IsColliderOnEnemy = false;
            _dronsActiveCount = 0;
            HideAll();
        }

        public void UpgradeDamage(int value)
        {
            DronsDamage += value;
        }

        public void UpgradeDroneSpeed(float value)
        {
            _speed += value;
        }

        public void RegisterNewDrone()
        {
            if (_dronsActiveCount == _drons.Count)
            {
                Debug.LogError("So Much Drone");
                return;
            }
            if (_dronsActiveCount == 0)
            {
                _isActive = true;
            }
            Drone drone = _drons[_dronsActiveCount];
            drone.ActivateDrone();
            drone.OnHandlerEnter += OnDroneHandler;

            _dronsActiveCount++;
        }
        private void OnDroneHandler(GameObject collider, GameObject drone) 
        {
            OnDroneHandlerEnter?.Invoke(collider, drone, DronsDamage);
        }
       

        public void HideAll()
        {
            foreach (var dron in _drons)
            {
                dron.DeactiveDrone();
            }
        }

        public void Update()
        {
            if (_isActive)
            {
                _selfObject.transform.position = _player.transform.position;
                _selfObject.transform.Rotate(0, 0, _speed * Time.deltaTime);
            }
        }

        private class Drone 
        {
            private GameObject _selfObject;
            private OnBehaviourHandler _onBehaviourHandler;
            public Action<GameObject, GameObject> OnHandlerEnter;
            public Drone(GameObject gameObject) 
            {
                _selfObject = gameObject;
                _onBehaviourHandler = _selfObject.transform.GetComponent<OnBehaviourHandler>();
            }

            public void DeactiveDrone() 
            {
                _selfObject.SetActive(false);
                _onBehaviourHandler.Trigger2DEntered -= OnBehaviorHandlerEnter;
            }

            public void ActivateDrone() 
            {
                _selfObject.SetActive(true);
                _onBehaviourHandler.Trigger2DEntered += OnBehaviorHandlerEnter;
            }

            private void OnBehaviorHandlerEnter(GameObject collider)
            {
                if (collider.tag != "Enemy")
                {
                    return;
                }
                OnHandlerEnter?.Invoke(collider, _selfObject);
            }
        }
    }
}

