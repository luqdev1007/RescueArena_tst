using System.Collections.Generic;
using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class SimpleObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 16;
        private readonly Queue<GameObject> available = new Queue<GameObject>();
        private readonly HashSet<GameObject> pooled = new HashSet<GameObject>();
        private void Awake()
        {
            for (int i = 0; i < initialSize; i++)
            {
                GameObject item = Create();
                item.SetActive(false);
                available.Enqueue(item);
                pooled.Add(item);
            }
        }
        private GameObject Create()
        {
            GameObject item = Instantiate(prefab, transform);
            PooledObject marker = item.GetComponent<PooledObject>();
            if (marker == null) marker = item.AddComponent<PooledObject>();
            marker.SetOwner(this);
            return item;
        }
        public GameObject Take(Vector3 position, Quaternion rotation)
        {
            GameObject item;
            if (available.Count == 0)
            {
                item = Create();
            }
            else
            {
                item = available.Dequeue();
                pooled.Remove(item);
            }
            item.transform.SetPositionAndRotation(position, rotation);
            item.SetActive(true);
            return item;
        }
        public void Return(GameObject item)
        {
            if (item == null || pooled.Contains(item)) return;
            item.SetActive(false);
            item.transform.SetParent(transform, false);
            available.Enqueue(item);
            pooled.Add(item);
        }
    }
}