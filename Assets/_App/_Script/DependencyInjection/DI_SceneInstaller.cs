using Zenject;

public class DI_SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<ILogger>().To<Logger>().AsSingle();
        Container.Bind<IAudioManager>().To<AudioManager>().FromComponentInNewPrefabResource(GlobalData.GetClassInManager<AudioManager>()).AsSingle();
        Container.Bind<ISocialManager>().To<SocialManager>().FromComponentInNewPrefabResource(GlobalData.GetClassInManager<SocialManager>()).AsSingle();
    }
}
