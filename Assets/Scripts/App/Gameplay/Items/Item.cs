using System;
using UnityEngine;
using static TandC.RunIfYouWantToLive.Common.Enumerators;

namespace TandC.RunIfYouWantToLive 
{
    public class Item
    {
        public event Action<Item> ItemDestroyHandler;
        public GameObject SelfObject;
        public ItemType ItemType;
        private OnBehaviourHandler _behaviourHandler;
        public int ItemValue;

        public Item(GameObject prefab, Transform parent, Vector2 spawnPosition, ItemType type, int itemValue) 
        {
            SelfObject = MonoBehaviour.Instantiate(prefab, parent);
            SelfObject.transform.position = spawnPosition;
            _behaviourHandler = SelfObject.GetComponent<OnBehaviourHandler>();
            _behaviourHandler.Trigger2DEntered += OnColliderHandler;
            ItemType = type;
            ItemValue = itemValue;
        }

        public void OnColliderHandler(GameObject collider) 
        {
            if(collider.tag == "Player")
            {
                ItemDestroyHandler?.Invoke(this);
            }
        }

        public void Update() 
        {
           
        }

        public void Dispose() 
        {
            MonoBehaviour.Destroy(SelfObject);
        }
    }
}

