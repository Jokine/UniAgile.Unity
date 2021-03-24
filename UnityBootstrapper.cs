using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UniAgile.Unity
{
    public abstract class UnityBootstrapper<T, TApplication, TConfiguration, TUnityConfiguration> : MonoBehaviour
        where T : UnityBootstrapper<T, TApplication, TConfiguration, TUnityConfiguration>
        where TApplication : Game.Application
        where TConfiguration : struct
        where TUnityConfiguration : IUnityConfiguration
    {
        [SerializeField]
        protected TConfiguration Configuration;

        [SerializeField]
        protected TUnityConfiguration UnityConfiguration;

        protected CancellationTokenSource ApplicationCancellationSource { get; private set; }
        protected CancellationTokenSource UICancellationSource { get; private set; }

        public bool IsRunning { get; protected set; }

        protected TApplication Application { get; private set; }
        protected IWindow ActiveWindow { get; private set; }


        protected abstract void Initialize();

        protected abstract void ProcessInputs(TApplication application);

        protected abstract TApplication CreateApplication(TConfiguration configuration);

        protected virtual async void Start()
        {
            Application.Start();
            IsRunning = true;

            ApplicationCancellationSource = new CancellationTokenSource();
            var applicationLoop =
                ApplicationLoop(FromTicksPerSecondToTimeSpan(UnityConfiguration.ApplicationTicksPerSecond),
                                ApplicationCancellationSource.Token);
            
            UICancellationSource = new CancellationTokenSource();
            var uiLoop =
                UILoop(UICancellationSource.Token);
            
            await Task.WhenAny(applicationLoop, uiLoop);

            Trace.WriteLine($"Terminating main loop for {GetType()}");
        }

        protected virtual void Awake()
        {
            gameObject.name = $"Bootstrapper.{GetType()}.{Guid.NewGuid().ToString()}";
            Trace.Write($"{gameObject.name} created");
            DontDestroyOnLoad(gameObject);
            Application = CreateApplication(Configuration);
            Initialize();  
            Time.fixedDeltaTime = (float)FromTicksPerSecondToTimeSpan(UnityConfiguration.ApplicationTicksPerSecond).TotalSeconds;
        }

        protected static TimeSpan FromTicksPerSecondToTimeSpan(double applicationTicksPerSecond)
        {
            return TimeSpan.FromSeconds(1d / applicationTicksPerSecond);
        }

        protected virtual async Task ApplicationLoop(TimeSpan deltaTime,
                                                     CancellationToken cancellationToken)
        {
            while (!ApplicationCancellationSource.Token.IsCancellationRequested)
            {
                try
                {
                    // todo: this should be elastic based on compensation
                    var startingTime = DateTime.Now;
                    ProcessInputs(Application);
                    await Application.Loop(deltaTime);
                    var executionTime = DateTime.Now - startingTime;
                    await Task.Delay(deltaTime - executionTime, cancellationToken);
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error with {nameof(ApplicationLoop)}. Exception {e}. Terminating");

                    throw;
                }
            }
        }


        protected virtual async Task UILoop(CancellationToken cancellationToken)
        {
            while (ActiveWindow != default)
            {
                try
                {
                    ActiveWindow = await ActiveWindow.ShowWindow(cancellationToken);
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error with {nameof(UILoop)}. Exception {e}. Terminating");

                    throw;
                }
            }
        }
    }

    // public class UnityBootstrapper : MonoBehaviour
    // {
    //     [SerializeField]
    //     protected BlowRunnerConfigurationSO BlowRunnerConfiguration;
    //
    //     private IWindow ActiveWindow;
    //
    //     private CancellationTokenSource ApplicationCancellationSource;
    //
    //     private TimeSpan ApplicationLoopInterval;
    //
    //     private UnityWindowLoader<CoreGameWindow> CoreGameWindow;
    //
    //     private UnityWindowLoader<MetaGameWindow> MetaGameWindow;
    //     private UnityWindowLoader<StatsWindow> StatsWindow;
    //     private CancellationTokenSource UICancellationSource;
    //     private TimeSpan UILoopDeltaInterval;
    //     private VirtualJoystickView VirtualJoystickView;
    //
    //     public static BlowRunnerApplication StaticBlowRunnerApplication { get; private set; }


        // private async void Start()
        // {
        //     BlowRunnerApplication.Start();
        //     IsRunning = true;
        //
        //     // await ApplicationLoop(ApplicationCancellationSource.Token, ApplicationLoopInterval);
        //
        //     await UILoop(UICancellationSource.Token, UILoopDeltaInterval);
        //
        //     // await Task.WhenAny(ApplicationLoop(ApplicationCancellationSource.Token, ApplicationLoopInterval), 
        //     //                    UILoop(UICancellationSource.Token, UILoopDeltaInterval));
        //     Trace.WriteLine($"Terminating main loop for {GetType()}");
        // }
        //
        // private async Task ApplicationLoop(CancellationToken cancellationToken,
        //                                    TimeSpan deltaTime)
        // {
        //     while (!ApplicationCancellationSource.Token.IsCancellationRequested)
        //     {
        //         // try
        //         // {
        //         var startingTime = DateTime.Now;
        //         await BlowRunnerApplication.Loop(deltaTime);
        //         var executionTime = DateTime.Now - startingTime;
        //         await Task.Delay(deltaTime - executionTime, cancellationToken);
        //
        //         // }
        //         // catch (Exception e)
        //         // {
        //         //     Trace.TraceError($"Error with {nameof(ApplicationLoop)}. Exception {e}. Terminating");
        //         //     throw;
        //         // }
        //     }
        // }
        //
        //
        // private Task Test<T>(ref T variable)
        // {
        //     while (true)
        //     {
        //         // if(variable ==)
        //     }
        // }
        //
        // private async Task UILoop(CancellationToken cancellationToken,
        //                           TimeSpan deltaTime)
        // {
        //     while (ActiveWindow != default)
        //     {
        //         // try
        //         // {
        //         ActiveWindow = await ActiveWindow.ShowWindow(cancellationToken);
        //
        //         // }
        //         // catch (Exception e)
        //         // {
        //         //     Trace.TraceError($"Error with {nameof(UILoop)}. Exception {e}. Terminating");
        //         //     throw;
        //         // }
        //     }
        // }
        //
        //
        //
        // public void Initialize(BlowRunnerConfigurationSO blowRunnerConfiguration)
        // {
        //     if (blowRunnerConfiguration == null)
        //     {
        //         throw new Exception($"{nameof(BlowRunnerConfigurationSO)} has not been set");
        //     }
        //
        //     ApplicationCancellationSource = new CancellationTokenSource();
        //     UICancellationSource = new CancellationTokenSource();
        //
        //     var config = blowRunnerConfiguration.BlowRunnerConfiguration;
        //     ApplicationLoopInterval = TimeSpan.FromMilliseconds(config.DeltaTimeMs);
        //     UILoopDeltaInterval = TimeSpan.FromMilliseconds(config.DeltaTimeMs);
        //
        //     var applicationModel = new ApplicationModel(new IRepository[0]);
        //
        //     var analyticsService =
        //         new BlowRunnerAnalytics(config.AnalyticsConfiguration, blowRunnerConfiguration.DevtodevConfiguration);
        //
        //     // BlowRunnerApplication = StaticBlowRunnerApplication =
        //     //                             new BlowRunnerApplication(applicationModel, config, analyticsService);
        //
        //     VirtualJoystickView = gameObject.AddComponent<VirtualJoystickView>();
        //
        //     VirtualJoystickView.Initialize(config.PlayerId);
        //
        //     MetaGameWindow =
        //         blowRunnerConfiguration.MetaGameWindowScene.CreateWindowLoader<MetaGameWindow>(window =>
        //             window.Initialize(CoreGameWindow));
        //
        //     CoreGameWindow =
        //         blowRunnerConfiguration.CoreGameWindowScene.CreateWindowLoader<CoreGameWindow>(window =>
        //             window.Initialize(MetaGameWindow, StatsWindow));
        //
        //     StatsWindow =
        //         blowRunnerConfiguration.StatsWindowScene.CreateWindowLoader<StatsWindow>(window =>
        //             window.Initialize(CoreGameWindow));
        //
        //     ActiveWindow = CoreGameWindow;
        // }
    // }
}