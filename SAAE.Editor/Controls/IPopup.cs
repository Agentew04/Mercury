using System.Threading.Tasks;

namespace SAAE.Editor.Controls;

public interface IPopup<in TRequest,TResult> {
    public Task<TResult> Request(TRequest request);
}