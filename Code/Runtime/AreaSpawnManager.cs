using Assets._KVLC_Project_Helix_Jump.Code.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code
{
    public enum PoolOperation
    {
        AcquireFromPool,
        ReturnToPool
    }

    public class AreaSpawnManager : MonoBehaviour
    {
        [SerializeField] private GameObject _cylinderObject;
        [SerializeField] private CylinderAreaHandler _prefab;
        [SerializeField] private Transform _prefabPlaceHolder;

        private float _height;
        private float _disableCheckerDuration;

        private float _timer;

        private const int _poolSize = 10;
        private const int _disableAreaLimit = 5; //< 10


        private const float _heightInterval = -1.5f;
        private const float _canBeSpawnedInComboPercentage = 0.25f;


        private List<CylinderAreaHandler> _pools = new List<CylinderAreaHandler>();

        private bool _tempBool;

        private void Start()
        {
            InitializePool();
            _pools.ForEach(r => r.InitializeArea());
        }

        private void InitializePool()
        {
            if (_prefab == null) return;

            for (int i = 0; i < _poolSize; i++)
            {
                CylinderAreaHandler area = Instantiate(_prefab, _prefabPlaceHolder);

                float offset = _heightInterval * i + _heightInterval;
                area.transform.position = new(0, offset, 0);

                _pools.Add(area);
                _height = i * _heightInterval + _heightInterval;
            }

         //   _cylinderObject.transform.position = new(0, _height, 0);
        }
        private void Update()
        {
            if (_pools.Count(r => !r.gameObject.activeSelf) > _disableAreaLimit)
            {
                List<CylinderAreaHandler> isFalseObjList = _pools.Where(r => !r.gameObject.activeSelf).ToList();

                if (CanBeSpawnedInCombo() && !_tempBool)
                {
                    isFalseObjList.ForEach(r => r.InitializeArea(false));
                    _disableCheckerDuration = Random.Range(10, 20);

                    _tempBool = true;
                }

                for (int i = 0; i < isFalseObjList.Count; i++)
                {
                    _height += _heightInterval;

                    isFalseObjList[i].transform.position = new(0, _height, 0);
                    isFalseObjList[i].gameObject.SetActive(true);
                }

               // _cylinderObject.transform.position = new(0, _height, 0);
            }

            if (_tempBool)
            {
                _timer += Time.deltaTime;
                if (_timer >= _disableCheckerDuration)
                {
                    _tempBool = false;
                    _timer = 0;
                }
            }
        }

        private bool CanBeSpawnedInCombo()
        {
            float probability = Random.value;
            return probability <= _canBeSpawnedInComboPercentage;
        }
    }
}