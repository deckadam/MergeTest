using Data;
using UnityEngine;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(fileName = "LevelInstaller", menuName = "Game Installers/LevelInstaller")]
    public class LevelInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private LevelList levelList;

        public override void InstallBindings()
        {
            Container.Bind<LevelList>().FromInstance(levelList).AsSingle();
        }
    }
}