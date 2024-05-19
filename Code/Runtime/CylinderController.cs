using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public class CylinderController : MonoBehaviour
    {
        [SerializeField] private float _rotationAmount;

        private void Update()
        {
            if (Input.GetMouseButton(0) && GameManager.Instance.Status == GameStatus.Start
                || GameManager.Instance.Status == GameStatus.Revive)
            {
                float horizontalRotation = Input.GetAxis("Mouse X") * _rotationAmount;
                transform.Rotate(Vector3.forward * (horizontalRotation * Time.deltaTime));
            }
        }
    }
}

