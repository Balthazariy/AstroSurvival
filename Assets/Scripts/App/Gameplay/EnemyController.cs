using System.Collections.Generic;
using UnityEngine;
using TandC.RunIfYouWantToLive.Common;
using System;

namespace TandC.RunIfYouWantToLive
{
    public class EnemyController : IController
    {
        private IGameplayManager _gameplayManager;
        private VFXController _vfxController;
        private PlayerController _playerController;
        public int ScoreCount => _scoreCount;
        private int _scoreCount;
        public Action<int> ScoreUpdateEvent;

        private List<Enemy> _enemies;

        private float _cooldownToSpawnEnemy;
        private GameplayData _gameplayData;
        private Transform _parent;
        private ObjectsController _objectController;
        private List<Enumerators.EnemyType> _enemysInPhase;
       // private float _phaseTimer;
        private Phase _currentPhase;
        public int CurrentPhaseIndex { get; private set; }
        public int DashDamage;

        private float _increaseEnemyParam;

        private float _camHeight;
        private float _camWidth;

        public EnemyController()
        {

        }

        public void Dispose()
        {
        }
        public bool HitEnemy(GameObject enemyObject, float damage, int dropChace)
        {
            Enemy enemy = null;
            foreach (Enemy enemyEnemy in _enemies)
            {
                if (enemyObject == enemyEnemy.SelfObject)
                {
                    enemy = enemyEnemy;
                    break;
                }
            }
            if(enemy == null) 
            {
                return false;
            }
            enemy.DeathEvent += OnEnemyDeath;
            enemy.DropChance = dropChace;
            enemy.TakeDamage(DamageHandler(damage, enemy.SelfObject));
            enemy.DeathEvent -= OnEnemyDeath;
            return true;
        }

        private void EnemyBehaviorHandler(GameObject gameObject, Enemy enemy) 
        {
            if (gameObject == _playerController.Player.SelfObject)
            {
                if (_playerController.Player.IsDashActive) 
                {
                    enemy.DeathEvent += OnEnemyDeath;
                    enemy.DropChance = _gameplayData.DropChance.DashChance;
                    enemy.TakeDamage(DamageHandler(_playerController.DashDamage, enemy.SelfObject));
                    enemy.DeathEvent -= OnEnemyDeath;
                    return;
                }
                _playerController.Player.TakeDamage(enemy.Damage);
                _vfxController.SpawnHitPlayerParticles(enemy.EnemyTransform.position, _playerController.Player.SelfObject.transform.position);
                if (!enemy.IsBoss) 
                {
                    _enemies.Remove(enemy);
                    enemy.Destroy();
                }
                return;
            }
            //if (gameObject == _vfxController.RocketBlowObject)
            //{
            //    enemy.DeathEvent += OnEnemyDeath;
            //    enemy.DropChance = _gameplayData.DropChance.RocketBlowChance;
            //    enemy.TakeDamage(DamageHandler(_playerController.RocketBlowDamage, enemy.SelfObject));
            //    enemy.DeathEvent -= OnEnemyDeath;
            //    return;
            //}
            //if (gameObject == _vfxController.BombBlowObject)
            //{
            //    enemy.DeathEvent += OnEnemyDeath;
            //    enemy.DropChance = _gameplayData.DropChance.BombBlowChance;
            //    enemy.TakeDamage(_playerController.BombDamage);
            //    enemy.DeathEvent -= OnEnemyDeath;
            //    return;
            //}
            //if (_playerController.Drones.IsColliderOnEnemy)
            //{
            //    if (_playerController.Drones.IsDrone(gameObject, out int damage))
            //    {
            //        enemy.DeathEvent += OnEnemyDeath;
            //        enemy.DropChance = _gameplayData.DropChance.DronChance;
            //        enemy.TakeDamage(DamageHandler(_playerController.Drones.DronsDamage, enemy.SelfObject));
            //        enemy.DeathEvent -= OnEnemyDeath;
            //        return;
            //    }
            //}
            //if (_objectController.IsLaserShot(gameObject, out float laserDamage))
            //{
            //    enemy.DeathEvent += OnEnemyDeath;
            //    enemy.DropChance = _gameplayData.DropChance.LaserShotChance;
            //    enemy.TakeDamage(DamageHandler(laserDamage, enemy.SelfObject));
            //    enemy.DeathEvent -= OnEnemyDeath;
            //    return;
            //}

            //foreach (var bullet in _objectController.InvokedBulletsList)
            //{
            //    if (gameObject == bullet.SelfObject)
            //    {
            //        if (bullet.IsRocket)
            //        {
            //            _vfxController.SpawnRocketBlow(bullet.SelfObject.transform.position);
            //        }
            //        enemy.DeathEvent += OnEnemyDeath;
            //        enemy.DropChance = _gameplayData.DropChance.StandartShotChance;
            //        enemy.TakeDamage(DamageHandler(bullet.Damage, enemy.SelfObject));
            //        _vfxController.SpawnHitParticles(bullet.SelfObject.transform.position, bullet.SelfObject.transform.eulerAngles);
            //        bullet.Dispose();
            //        _objectController.InvokedBulletsList.Remove(bullet);
            //        enemy.DeathEvent -= OnEnemyDeath;
            //        break;
            //    }
            //}
        }
        private float DamageHandler(float damage, GameObject enemy) 
        {
            int chance = UnityEngine.Random.Range(0, 101);
            bool isCriticalChance = _playerController.CriticalChanceProcent >= chance;
            if (isCriticalChance)
            {
                damage *= _playerController.CriticalDamageMultiplier;
                _vfxController.SpawnDomagePointVFX(enemy.transform.position, damage, Color.red);
            }
            else 
            {
                _vfxController.SpawnDomagePointVFX(enemy.transform.position, damage, Color.yellow);
            }
            return damage;
        }
        private void OnEnemyDeath(Enemy enemy) 
        {
            if(enemy.EnemyType == Enumerators.EnemyType.PiciesFull) 
            {
                float size = enemy.EnemyTransform.localScale.x;
                var firstHalf = SpawnEnemy(_gameplayData.GetEnemiesByType(Enumerators.EnemyType.PiciesHalf), false);
                firstHalf.EnemyTransform.position = new Vector2(enemy.EnemyTransform.position.x - 6 * size, enemy.EnemyTransform.position.y);
                firstHalf.EnemyTransform.localScale = new Vector2(firstHalf.EnemyTransform.localScale.x * -1, firstHalf.EnemyTransform.localScale.y);
                var secondHalf = SpawnEnemy(_gameplayData.GetEnemiesByType(Enumerators.EnemyType.PiciesHalf), false);
                secondHalf.EnemyTransform.position = new Vector2(enemy.EnemyTransform.position.x + 6 * size, enemy.EnemyTransform.position.y);
                _enemies.Add(firstHalf);
                _enemies.Add(secondHalf);
            }
            if(enemy.EnemyType == Enumerators.EnemyType.PiciesHalf) 
            {
                float size = enemy.EnemyTransform.localScale.x;
                var firstHalf = SpawnEnemy(_gameplayData.GetEnemiesByType(Enumerators.EnemyType.PiciesSmall), false);
                firstHalf.EnemyTransform.position = new Vector2(enemy.EnemyTransform.position.x, enemy.EnemyTransform.position.y + 6 * size);
                firstHalf.EnemyTransform.localScale = new Vector2(size, size);
                var secondHalf = SpawnEnemy(_gameplayData.GetEnemiesByType(Enumerators.EnemyType.PiciesSmall), false);
                secondHalf.EnemyTransform.position = new Vector2(enemy.EnemyTransform.position.x, enemy.EnemyTransform.position.y - 6 * size);
                secondHalf.EnemyTransform.localScale = new Vector2(size, size * -1);
                _enemies.Add(firstHalf);
                _enemies.Add(secondHalf);
            }
            _vfxController.SpawnDeathParticles(enemy.EnemyTransform.position);
            _objectController.OnEnemyDeath(enemy);
            _scoreCount += enemy.GainedExperience;
            ScoreUpdateEvent?.Invoke(_scoreCount);
            enemy.Destroy();
            _enemies.Remove(enemy);
        }

        public void FrozeAllEnemy() 
        {
            for (int i = 0; i < _enemies.Count; i++) 
            {
                var enemy = _enemies[i];
                _vfxController.SpawnFrozeEnemyVFX(enemy.SelfObject.transform.position, enemy.SelfObject.transform);
                enemy.FrozeEnemy();
            }
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _objectController = _gameplayManager.GetController<ObjectsController>();
            _gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;

        }

        private Enemy SpawnEnemy(Enemies data, bool isBoss) 
        {
            Enemy enemy;

            switch (data.movementType)
            {
                case Enumerators.EnemyMovementType.ChainSawEnemy:
                    enemy = new ChainSawEnemy(_parent, data, _playerController.Player.SelfTransform, SetSpawnPosition(), isBoss);
                    break;
                default:
                    enemy = new DefaultEnemy(_parent, data, _playerController.Player.SelfTransform, SetSpawnPosition(), isBoss);
                    break;
            }
            enemy.IncreaseParam(_increaseEnemyParam);
            enemy.OnCollderEvent += EnemyBehaviorHandler;
            enemy.OnEndEnemyLifeTime += EnemyLifeTimeEnd;
            enemy.DestroyEvent += EnemyDestroyedEventHandler;
            return enemy;

        }

        private void EnemyLifeTimeEnd(Enemy enemy) 
        {
            if(Vector2.Distance(enemy.SelfObject.transform.position, _playerController.Player.SelfTransform.position) >= 1500f) 
            {
                enemy.IsEndLifeTime = true;
            }
            else 
            {
                enemy.AddLifeTime(10f);
            }
        }

        public void StartSpawnEnemy()
        {
            if (_enemies.Count >= 50) 
            {
                return;
            }
            int enemyInPhaseId = UnityEngine.Random.Range(0, _enemysInPhase.Count);
            var enemyType = _enemysInPhase[enemyInPhaseId];
            _enemysInPhase.Remove(enemyType);
            _enemies.Add(SpawnEnemy(_gameplayData.GetEnemiesByType(enemyType), false));
        }

        private void SetNewPhase(int phaseId)
        {
            if (CurrentPhaseIndex >= _gameplayData.gamePhases.Length)
            {
                IncreasePhaseIndex();
            }
            var miniBoss = _gameplayData.GetMiniBossByPhaseId(phaseId);
            if(miniBoss != null) 
            {
                _enemies.Add(SpawnEnemy(miniBoss.enemyData, true)); 
            }
            CurrentPhaseIndex = phaseId;
            _currentPhase = _gameplayData.GetPhaseById(phaseId);
            _enemysInPhase = new List<Enumerators.EnemyType>();
            if (_currentPhase != null)
            {
                for(int i = 0; i < _currentPhase.enemyInPhase.Length; i++) 
                {
                    for(int j = 0; j < _currentPhase.EnemyCount[i]; j++) 
                    {
                        _enemysInPhase.Add(_currentPhase.enemyInPhase[i]);
                    }
                }
                _cooldownToSpawnEnemy = _currentPhase.spawnTime; 
            }
        }

        private Vector2 SetSpawnPosition()
        {
            Vector3 screenSize = new Vector3(_camWidth + 100, _camHeight + 100);
            Vector2 maxStartPosition = _playerController.Player.SelfTransform.position + screenSize;
            Vector2 minStartPosition = _playerController.Player.SelfTransform.position - screenSize;
            int chance = UnityEngine.Random.Range(1, 3);
            Vector2 firstPosition = new Vector2(0, 0);
            if (chance == 1)
                firstPosition = maxStartPosition;
            else if (chance == 2)
                firstPosition = minStartPosition;

            chance = UnityEngine.Random.Range(1, 3);
            Vector2 position = new Vector2(0, 0);
            if (chance == 1)
                position = new Vector2(UnityEngine.Random.Range(minStartPosition.x, maxStartPosition.x), firstPosition.y);
            else if (chance == 2)
                position = new Vector2(firstPosition.x, UnityEngine.Random.Range(minStartPosition.y, maxStartPosition.y));

            return position;
        }

        public void EnemyDestroyedEventHandler(Enemy enemy)
        {
            _enemies.Remove(enemy);
        }

        private void GameplayStartedEventHandler()
        {

            float screenAspect = (float)Screen.width / (float)Screen.height;
            _camHeight = _gameplayManager.GameplayCamera.orthographicSize;
            _camWidth = screenAspect * _camHeight;
            _parent = _gameplayManager.GameplayObject.transform.Find("[Enemy]");
            _enemies = new List<Enemy>();
            _gameplayData = _gameplayManager.GameplayData;
            _scoreCount = 0;
            CurrentPhaseIndex = 0;
            _increaseEnemyParam = 1f;
            SetNewPhase(CurrentPhaseIndex);
            ScoreUpdateEvent?.Invoke(_scoreCount);
        }

        public void IncreasePhaseIndex() 
        {
            CurrentPhaseIndex++;
            if(CurrentPhaseIndex >= _gameplayData.gamePhases.Length - 1) 
            {
                _increaseEnemyParam += 0.25f;
                CurrentPhaseIndex = 0;
            }
            SetNewPhase(CurrentPhaseIndex);
        }

        public void ResetAll()
        {
            _enemies.Clear();
        }

        public void Update()
        {
            if (!_gameplayManager.IsGameplayStarted)
                return;
            //_phaseTimer -= Time.deltaTime;
            //if(_phaseTimer <= 0) 
            //{
            //    IncreasePhaseIndex();
            //}
            _cooldownToSpawnEnemy -= Time.deltaTime;
            if (_cooldownToSpawnEnemy <= 0)
            {
                _cooldownToSpawnEnemy = _currentPhase.spawnTime;
                StartSpawnEnemy();
                if(_enemysInPhase.Count <= 0) 
                {
                    // Debug.Log("Enemis count <= 0");
                    IncreasePhaseIndex();
                }
            }

            //foreach (var item in _enemies) 
            //{
            //    item.Update();
            //}
            for(int i = 0; i < _enemies.Count; i++) 
            {
                var enemy = _enemies[i];
                if(enemy.IsEndLifeTime) 
                {
                    enemy.Destroy();
                    _enemies.Remove(enemy);
                }
                enemy.Update();
            }
                
        }

        public void FixedUpdate()
        {
            if (!_gameplayManager.IsGameplayStarted)
                return;


            if (_enemies.Count > 0)
                foreach (var item in _enemies)
                    item.FixedUpdate();
        }
    }
}