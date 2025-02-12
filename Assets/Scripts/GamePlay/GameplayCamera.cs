using Cinemachine;
using UnityEngine;
using Zenject;
using CameraData = Data.Camera.CameraData;

namespace GamePlay
{
    public class GameplayCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera vCam;
        [SerializeField] private CinemachineTargetGroup targetGroup;

        private CameraData _cameraData;

        [Inject]
        private void Inject(CameraData cameraData)
        {
            _cameraData = cameraData;
        }

        public void Initialize(Vector2Int levelSize)
        {
            var min = new GameObject();
            min.transform.parent = transform;
            min.transform.localPosition = new Vector3(0, 0);


            var max = new GameObject();
            max.transform.parent = transform;
            max.transform.localPosition = new Vector3(levelSize.x, levelSize.y);

            targetGroup.AddMember(min.transform, 1f, _cameraData.Padding);
            targetGroup.AddMember(max.transform, 1f, _cameraData.Padding);

            vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_GroupFramingSize = _cameraData.FramingSize;
        }
    }
}