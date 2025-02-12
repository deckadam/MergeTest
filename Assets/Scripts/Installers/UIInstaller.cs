using UI;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class UIInstaller : MonoInstaller
    {
        [SerializeField] private PiecePickerUI piecePickerUI;

        public override void InstallBindings()
        {
            Container.BindInstance(piecePickerUI).AsSingle();
        }
    }
}