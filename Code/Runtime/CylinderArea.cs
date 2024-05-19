using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{

    public class CylinderArea : SerializedMonoBehaviour, ICylinderZone
    {
        [SerializeField] private CylinderAreaHandler _handler;

        private MeshRenderer _renderer;
        private MeshCollider _meshCollider;

        [SerializeField]
        public CylinderZoneType Type { get; private set; }

        public void SetZone(CylinderZoneType type, Color color)
        {
            this.Type = type;

            _renderer ??= GetComponent<MeshRenderer>();
            _meshCollider ??= GetComponent<MeshCollider>();

            _meshCollider.isTrigger = type == CylinderZoneType.Jumpable;
            _renderer.enabled = type != CylinderZoneType.Jumpable;

            _renderer.material.color = color;
        }

        public void Break()
        {
            _handler.Break();
        }

        public void SetCollider(bool isEnable)
        {
            _meshCollider ??= GetComponent<MeshCollider>();
            _meshCollider.enabled = isEnable;
        }
    }
}