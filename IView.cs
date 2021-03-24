using System.Threading;
using System.Threading.Tasks;

namespace UniAgile.Unity
{
    public interface IView
    {
        Task Show(CancellationToken cancellationToken);
    }

    public interface IView<T> : IView
        where T : IView<T>
    {
        Task<T> GetView(CancellationToken cancellationToken);
    }
}