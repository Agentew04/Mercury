using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Services;

public abstract class BaseService<T> {

    private ILogger<T>? logger;
    /// <summary>
    /// Gets the class internal logging instance.
    /// </summary>
    protected ILogger<T> Logger => logger ??= App.Services.GetRequiredService<ILogger<T>>();
}