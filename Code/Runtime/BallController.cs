using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Zenject;
using System.Collections.Generic;
using UnityEngine.Android;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    [RequireComponent(typeof(NotifyPlayerScore))]
    public class BallController : Singleton<BallController>
    {
        [Header("Ground Check Settings")]
        [SerializeField] private float _rayDistance;
        [SerializeField] private float _jumpForce;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private List<GameObject> _ballCells = new();
        [SerializeField] private GameObject _ballObject;

        private Rigidbody _rigidBody;
        private CinemachineImpulseSource _impulse;
        private TrailRenderer _trailRenderer;
        private MeshRenderer _ballMeshRenderer;

        [Inject] private ISoundHandler _soundHandler;
        [Inject] private IGameControl _gameStatus;

        private float _lastJumpTime;
        private const float JumpInterval = 0.5f;

        private const string _ballMatEmissionKeyword = "_EMISSION";

        private void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _impulse = GetComponent<CinemachineImpulseSource>();
            _trailRenderer = GetComponent<TrailRenderer>();
            _ballMeshRenderer = _ballObject.GetComponent<MeshRenderer>();

            _lastJumpTime = Time.time;
        }

        private void Update()
        {
            _trailRenderer.enabled = _gameStatus.Status == GameStatus.Start;
            _rigidBody.isKinematic = _gameStatus.Status != GameStatus.Start
                && _gameStatus.Status != GameStatus.Revive;

            if (_gameStatus.Status == GameStatus.Revive)
                HandleBallSetAlphaDisable();
            else
                HandleBallSetAlphaEnable();

            if ((_gameStatus.Status == GameStatus.Start || _gameStatus.Status == GameStatus.Revive)
                && !_ballObject.activeSelf)
                ActivateBall();

            if (_rigidBody.isKinematic)
                return;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, _rayDistance, _layerMask, QueryTriggerInteraction.Ignore))
                if (Time.time - _lastJumpTime >= JumpInterval)
                    PerformJump(hitInfo);
        }

        private void ActivateBall()
        {
            _ballObject.SetActive(true);

            if (_gameStatus.Status == GameStatus.Start)
                HandleBallSetAlphaEnable();

            if (_trailRenderer != null)
                _trailRenderer.enabled = true;

            _ballCells.ForEach(r =>
            {
                r.SetActive(false);
                r.transform.localPosition = Vector3.zero;
                r.transform.rotation = Quaternion.Euler(-90, 0, 0);
            });
        }

        private void PerformJump(RaycastHit hitInfo)
        {
            _rigidBody.velocity = Vector3.up * _jumpForce;

            if (_ballObject != null && transform != null)
                transform.DOPunchScale(Vector3.one * 0.08f, 0.5f).SetEase(Ease.OutQuad);

            Vector3 splashOffset = hitInfo.point + Vector3.up * 0.01f;
            Transform splashPlaceholder = hitInfo.collider.transform;

            SplashVisualHandler.Instance?.GetPoolObject(PoolOperation.AcquireFromPool, splashOffset, splashPlaceholder);
            _soundHandler.PlayOneShot(0, false);

            _impulse.GenerateImpulse();
            _lastJumpTime = Time.time;

            ICylinderZone cylinderZone = hitInfo.collider.GetComponent<ICylinderZone>();

            if (cylinderZone != null)
                CylinderZoneAction(cylinderZone);
        }

        private void CylinderZoneAction(ICylinderZone cylinderZone)
        {
            if (cylinderZone == null) return;

            switch (cylinderZone.Type)
            {
                case CylinderZoneType.Jumpable:
                    cylinderZone.Break();
                    break;
                case CylinderZoneType.Safe:
                    break;
                case CylinderZoneType.Red:
                    HandleRedZoneCollision();
                    break;
            }
        }

        private void HandleRedZoneCollision()
        {
            if (_ballObject == null) return;

            _ballObject.SetActive(false);
            _ballCells.ForEach(r => r.SetActive(true));

            if (_trailRenderer != null)
                _trailRenderer.enabled = false;

            _gameStatus.UpdateGameStatus(GameStatus.GameOver);
        }
        private void HandleBallSetAlphaDisable()
        {
            _ballMeshRenderer.material.DisableKeyword(_ballMatEmissionKeyword);
            Color ballColor = _ballMeshRenderer.material.color;

            float t = Mathf.PingPong(Time.time / 1f, 1f);
            ballColor.a = t;

            _ballMeshRenderer.material.color = ballColor;


        }
        private void HandleBallSetAlphaEnable()
        {
            _ballMeshRenderer.material.EnableKeyword(_ballMatEmissionKeyword);
            Color ballColor = _ballMeshRenderer.material.color;
            ballColor.a = 1;

            _ballMeshRenderer.material.color = ballColor;
        }
        private void OnTriggerEnter(Collider other)
        {
            ICylinderZone cylinderZone = other.gameObject.GetComponent<ICylinderZone>();
            CylinderZoneAction(cylinderZone);
        }
    }
}
