using Zenject;
using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public class DependencyInjectionInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IPlayerScoreProvider>().To<NotifyPlayerScore>().FromInstance(NotifyPlayerScore.Instance).AsSingle();
            Container.Bind<IGameControl>().To<GameManager>().FromInstance(GameManager.Instance).AsSingle();
            Container.Bind<ICylinderColorFactoryHandler>().To<CylinderColorFactory>().FromInstance(CylinderColorFactory.Instance).AsSingle();
            Container.Bind<ISoundHandler>().To<SoundManager>().FromInstance(SoundManager.Instance).AsSingle();
            Container.Bind<ISaveProvider>().To<SaveManager>().FromInstance(SaveManager.Instance).AsSingle();
            Container.Bind<IUIHandler>().To<UIManager>().FromInstance(UIManager.Instance).AsSingle();
        }
    }
}