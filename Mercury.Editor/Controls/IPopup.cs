using System.Threading.Tasks;

namespace Mercury.Editor.Controls;

public interface IPopup<in TRequest,TResult> {
    public Task<TResult> RequestAsync(TRequest request);
}