using Assets._KVLC_Project_Helix_Jump.Code;
using Assets._KVLC_Project_Helix_Jump.Code.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SplashVisualHandler : Singleton<SplashVisualHandler>
{
    [SerializeField] private GameObject _prefab;

    private List<GameObject> _pools = new();

    private const int _poolSize = 15;

    private void Start()
    {
        InitializePool();
    }
    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject splash = Instantiate(_prefab);
            splash.SetActive(false);
            _pools.Add(splash);
        }
    }

    public GameObject GetPoolObject(PoolOperation operation, Vector3 position, Transform parent)
    {
        bool check = operation == PoolOperation.AcquireFromPool;
        GameObject splash = _pools.FirstOrDefault(r => check ? !r.activeSelf : r.activeSelf);

        if (check)
        {
            splash.transform.SetParent(parent, false);
            splash.transform.position = position;
        }

        splash.SetActive(check);
        return splash;
    }
}
