using GamePlay;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [FormerlySerializedAs("piecePickerUI")] [SerializeField] private GamePlayUI gamePlayUI;
        [SerializeField] private StartUI startUI;
        [SerializeField] private GameManager gm;
        [SerializeField] private NextLevelUI nextLevelUI;
        [SerializeField] private ReloadUI reloadUI;
        
        public override void InstallBindings()
        {
            Container.BindInstance(gamePlayUI).AsSingle();
            Container.BindInstance(nextLevelUI).AsSingle();
            Container.BindInstance(reloadUI).AsSingle();
            Container.BindInstance(startUI).AsSingle();
            Container.BindInstance(gm).AsSingle();
        }
    }
}