using UnityEngine;

namespace ToySRP
{
    public sealed class CubemapCameraHolder : MonoBehaviour
    {
        [SerializeField] Camera camera;
        public Camera Camera => camera;

        void Start()
        {
            camera.enabled = false;
        }
    }
}
