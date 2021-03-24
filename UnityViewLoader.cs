using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniAgile.Unity
{
    public static class UnityViewLoaderExtensions
    {
        public static UnityViewLoader<T> CreateLoader<T>(this string sceneName,
                                                         Func<T, T> initialize = default)
            where T : UnityView<T>, IView<T>, IDisposable
        {
            return new UnityViewLoader<T>(sceneName, initialize);
        }

        public static UnityViewLoader<T> CreateLoader<T>(Func<T, T> initialize = default)
            where T : UnityView<T>, IView<T>, IDisposable
        {
            return typeof(T).ToString().CreateLoader(initialize);
        }

        public static UnityWindowLoader<T> CreateWindowLoader<T>(this string sceneName,
                                                                 Func<T, T> initialize = default)
            where T : UnityView<T>, IWindow<T>, IDisposable
        {
            return new UnityWindowLoader<T>(sceneName, initialize);
        }

        public static UnityWindowLoader<T> CreateWindowLoader<T>(Func<T, T> initialize = default)
            where T : UnityView<T>, IWindow<T>, IDisposable
        {
            return typeof(T).ToString().CreateWindowLoader(initialize);
        }
    }

    public class UnityWindowLoader<T> : UnityViewLoader<T>, IWindow<T>
        where T : UnityView<T>, IWindow<T>, IDisposable
    {
        public UnityWindowLoader(string sceneName,
                                 Func<T, T> initialize = default) : base(sceneName, initialize)
        {
        }

        public async Task<IWindow> ShowWindow(CancellationToken cancellationToken)
        {
            using var view = await GetView(cancellationToken);

            return await view.ShowWindow(cancellationToken);
        }
    }

    public class UnityViewLoader<T> : IView<T>
        where T : UnityView<T>, IView<T>, IDisposable
    {
        public UnityViewLoader(string sceneName,
                               Func<T, T> initialize = default)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException($"Scene {sceneName} couldn't be found");
            }

            SceneName = sceneName;
            Initialize = initialize;
        }

        private readonly List<GameObject> CachedGameobjectList = new List<GameObject>();
        private readonly Func<T, T> Initialize;

        private readonly string SceneName;

        /// <summary>
        ///     Can be null
        /// </summary>
        private Scene ActiveScene;

        /// <summary>
        ///     Can be null
        /// </summary>
        private T CurrentView;

        public async Task Show(CancellationToken cancellationToken)
        {
            using var view = await GetView(cancellationToken);

            await view.Show(cancellationToken);
        }

        public async Task<T> GetView(CancellationToken cancellationToken)
        {
            var operation = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            operation.allowSceneActivation = true;
            await operation.WaitFoCompletion(cancellationToken);

            // these are necessary to wait for the scene to be properly loaded and have root game objects in place
            await Task.Yield();
            await Task.Yield();
            ActiveScene = SceneManager.GetSceneByName(SceneName);
 
            ActiveScene.GetRootGameObjects(CachedGameobjectList);

            foreach (var go in CachedGameobjectList)
            {
                var maybeView = go.GetComponent<T>();

                if (maybeView != null)
                {
                    CurrentView = maybeView;

                    break;
                }
            }

            CachedGameobjectList.Clear();

            if (CurrentView == null)
            {
                throw new
                    Exception($"Unable to find gameobject with component {typeof(T)} from scene {ActiveScene.name}. Make sure the scene is built correctly");
            }

            CurrentView.ActiveScene = ActiveScene;
            Initialize?.Invoke(CurrentView);

            return CurrentView;
        }
    }
}