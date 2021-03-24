using System.Threading;
using System.Threading.Tasks;

namespace UniAgile.Unity
{
    public interface IWindow
    {
        Task<IWindow> ShowWindow(CancellationToken cancellationToken);
    }

    public interface IWindow<T> : IWindow, IView<T>
        where T : IView<T>
    {
    }
}