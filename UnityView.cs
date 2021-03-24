using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniAgile.Unity
{
    public abstract class UnityView<T> : MonoBehaviour, IView<T>
        where T : UnityView<T>, IView<T>, IDisposable
    {
        internal Scene ActiveScene;

        public Task<T> GetView(CancellationToken cancellationToken)
        {
            return Task.FromResult((T) this);
        }

        public abstract Task Show(CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            if (ActiveScene == default)
            {
                Destroy(gameObject);
            }
            else
            {
                SceneManager.UnloadSceneAsync(ActiveScene);
                ActiveScene = default;
            }
        }
    }
}