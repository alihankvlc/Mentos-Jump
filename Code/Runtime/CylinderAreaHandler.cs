using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using System.Collections;
using Zenject;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public enum CylinderZoneType { Jumpable, Safe, Red }

    public interface ICylinderZone
    {
        public CylinderZoneType Type { get; }
        public void SetZone(CylinderZoneType type, Color color);
        public void Break();
    }

    public class CylinderAreaHandler : MonoBehaviour
    {
        [SerializeField, InlineEditor] private List<CylinderArea> _areaList = new List<CylinderArea>();

        private Animator _animator;

        private readonly int _breakHashId = Animator.StringToHash("Break");
        private bool _isBreak;

        private ISoundHandler _soundHandler;

        private IPlayerScoreProvider _scoreProvider;
        private ICylinderColorFactoryHandler _colorFactoryHandler;

        [Inject]
        private void Constructor(IPlayerScoreProvider scoreProvider, ICylinderColorFactoryHandler colorFactoryHandler,
            ISoundHandler soundHandler)
        {
            if (scoreProvider != null && colorFactoryHandler != null)
            {
                _scoreProvider = scoreProvider;
                _colorFactoryHandler = colorFactoryHandler;
                _soundHandler = soundHandler;
            }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }
        public void InitializeArea(bool isRandom = true)
        {
            _areaList.ForEach(r => r.SetCollider(true));

            Init_Cylinder_Safe_Area();
            Init_Cylinder_Red_Area(isRandom);
            Init_Jumpable_Area(isRandom);

        }

        public void Init_Cylinder_Red_Area(bool isEnable = true)
        {
            while (_areaList.Count(r => r.Type == CylinderZoneType.Red) < CalculateProbability() && isEnable)
            {
                int randomIndex = Random.Range(0, _areaList.Count);
                _areaList[randomIndex].SetZone(CylinderZoneType.Red, _colorFactoryHandler.RedColor);
            }
        }

        private void Init_Cylinder_Safe_Area()
        {
            foreach (CylinderArea area in _areaList)
            {
                if (area.Type == CylinderZoneType.Red || area.Type == CylinderZoneType.Jumpable)
                    continue;

                area.SetZone(CylinderZoneType.Safe, _colorFactoryHandler.SafeColor);
            }
        }

        private void Init_Jumpable_Area(bool isRandom = true)
        {
            if (!isRandom)
            {
                int randomIndex = Random.Range(0, _areaList.Count);
                _areaList[randomIndex].SetZone(CylinderZoneType.Jumpable, _colorFactoryHandler.SafeColor);
                return;
            }

            while (_areaList.Count(r => r.Type == CylinderZoneType.Jumpable) < CalculateProbability())
            {
                int randomIndex = Random.Range(0, _areaList.Count);
                _areaList[randomIndex].SetZone(CylinderZoneType.Jumpable, _colorFactoryHandler.SafeColor);
            }

        }

        private int CalculateProbability()
        {
            float probability = Random.value;

            return probability switch
            {
                <= 0.60f => 2,
                <= 0.70f => 1,
                _ => 1,
            };
        }

        public void Break()
        {
            if (!_isBreak)
            {
                StartCoroutine(DisableThisGameObject());

                _animator.SetBool(_breakHashId, true);
                _areaList.ForEach(r => r.SetCollider(false));

                _isBreak = true;

                _scoreProvider.UpdateScore();
                _soundHandler.PlayOneShot(2, true);
            }

        }

        private IEnumerator DisableThisGameObject()
        {
            yield return new WaitForSeconds(0.25f);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _isBreak = false;
        }
    }
}
