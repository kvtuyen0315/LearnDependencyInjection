using Zenject;

public class DI_SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        base.InstallBindings();

        Container.Bind<ILogger>().To<Logger>().AsSingle();
        Container.Bind<IAudioManager>().To<AudioManager>().AsSingle();
        Container.Bind<ISocialManager>().To<SocialManager>().AsSingle();
    }
}
