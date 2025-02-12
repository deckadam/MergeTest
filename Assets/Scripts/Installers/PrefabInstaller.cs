using GamePlay;
using UnityEngine;
using Zenject;


namespace Installers
{
    [CreateAssetMenu(fileName = "PrefabInstaller", menuName = "Game Installers/PrefabInstaller")]
    public class PrefabInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private GameplayLevel levelPrefab;

        public override void InstallBindings()
        {
            Container.Bind<GameplayLevel>().FromInstance(levelPrefab).AsSingle();
        }
    }
}