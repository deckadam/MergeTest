using UnityEngine;

namespace Data.Camera
{
    [CreateAssetMenu(fileName = "Camera Data", menuName = "Camera/CameraData",order = 0)]
    public class CameraData:ScriptableObject
    {
        [SerializeField] private float framingSize;
        [SerializeField] private float padding;

        public float FramingSize => framingSize;
        public float Padding => padding;
    }
}