using UnityEngine;
using Zenject;
using CameraData = Data.Camera.CameraData;

namespace Installers
{
    [CreateAssetMenu(fileName = "DataInstaller", menuName = "Game Installers/DataInstaller")]
    public class DataInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private CameraData cameraData;

        public override void InstallBindings()
        {
            Container.Bind<CameraData>().FromInstance(cameraData);
        }
    }
}