using System.Collections.Generic;
using UnityEngine;

public static class GameObjectPool
{
    private static GameObject PoolAttach;
    private static Dictionary<GameObject, Queue<GameObject>> s_poolMap = new Dictionary<GameObject, Queue<GameObject>>();
    private static Dictionary<GameObject, GameObject> s_instanceToSrc = new Dictionary<GameObject, GameObject>();
    public static GameObject GetInstance(GameObject prefabSrc, Transform parent)
    {
        if (PoolAttach == null)
        {
            PoolAttach = new GameObject("[GameObjectPool]");
            PoolAttach.transform.localScale = Vector3.zero;
            GameObject.DontDestroyOnLoad(PoolAttach);
        }

        if (!s_poolMap.ContainsKey(prefabSrc)) s_poolMap[prefabSrc] = new Queue<GameObject>();

        GameObject instance = null;
        var pool = s_poolMap[prefabSrc];
        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
            instance.transform.SetParent(parent, true);
            instance.transform.localScale = prefabSrc.transform.localScale;
            instance.transform.localRotation = prefabSrc.transform.localRotation;
            instance.transform.localPosition = prefabSrc.transform.localPosition;
        }
        else
        {
            instance = GameObject.Instantiate(prefabSrc, parent);
            s_instanceToSrc[instance] = prefabSrc;
        }

        //instance.SetActive(true);
        return instance;
    }

    public static void Release(GameObject instance)
    {
        GameObject src;
        Queue<GameObject> pool;
        s_instanceToSrc.TryGetValue(instance, out src);
        if (src != null && s_poolMap.TryGetValue(src, out pool))
        {
            pool.Enqueue(instance);
            //instance.SetActive(false);
            if (instance == null) return;
            if (PoolAttach == null) return;

            instance.transform.SetParent(PoolAttach.transform, true);
        }
        else
        {
            GameObject.Destroy(instance);
        }
    }
}
