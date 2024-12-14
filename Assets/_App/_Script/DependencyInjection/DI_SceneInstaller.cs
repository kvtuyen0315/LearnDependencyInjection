using Zenject;

public class DI_SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        base.InstallBindings();

        Container.Bind<ILogger>().To<Logger>();
        Container.Bind<IAudioManager>().To<AudioManager>();
        Container.Bind<ISocialManager>().To<SocialManager>();
    }
}
