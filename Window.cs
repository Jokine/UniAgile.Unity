using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace UniAgile.Unity
{
    public abstract class Window<T> : UnityView<T>, IWindow<T>, IDisposable
        where T : UnityView<T>, IView<T>, IDisposable
    {
        public async Task<IWindow> ShowWindow(CancellationToken cancellationToken)
        {
            Debug.WriteLine($"Showing {GetType()}", GetType().Name);
            gameObject.SetActive(true);

            var result = await ExitTask(cancellationToken);

            gameObject.SetActive(false);
            Debug.WriteLine($"Closing {GetType()}", GetType().Name);

            return result;
        }

        public override async Task Show(CancellationToken cancellationToken)
        {
            await ShowWindow(cancellationToken);
        }


        protected abstract Task<IWindow> ExitTask(CancellationToken cancellationToken);
    }
}