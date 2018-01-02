using Panda.Client;

namespace Panda.AppLauncher
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents an object that is capable of creating context menu items for RegisteredApplications
    /// </summary>
    /// <seealso cref="T:Panda.AppLauncher.RegisteredApplication" />
    public interface IRegisteredApplicationContextMenuProvider : IContextMenuProvider<RegisteredApplication>
    {
    }
}