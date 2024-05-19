using System.Collections;
using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public interface ICylinderColorFactoryHandler
    {
        public Color SafeColor { get; }
        public Color RedColor { get; }
    }

    public class CylinderColorFactory : Singleton<CylinderColorFactory>, ICylinderColorFactoryHandler
    {
        [SerializeField] private Color _safeColor;
        [SerializeField] private Color _redColor;

        public Color SafeColor => _safeColor;
        public Color RedColor => _redColor;
    }
}