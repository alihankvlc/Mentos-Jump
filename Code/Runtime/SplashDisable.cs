using DG.Tweening;
using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public class SplashDisable : MonoBehaviour
    {
        private const float _disableDuration = 1.5f;

        private void OnEnable() => Invoke(nameof(DisableGameObject), _disableDuration);
        private void DisableGameObject() => gameObject.SetActive(false);
    }
}
