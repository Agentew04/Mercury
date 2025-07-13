using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SAAE.Editor.Services;

public abstract class BaseService<T> {

    protected static ILogger<T> GetLogger() {
        return App.Services.GetRequiredService<ILogger<T>>();
    }
}