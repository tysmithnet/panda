using System.IO;

namespace Panda.Client
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents an object that is capable of creating ContextMenu items given some FileInfo objects
    /// </summary>
    /// <seealso cref="!:Panda.Client.IContextMenuProvider{System.IO.FileInfo}" />
    public interface IFileSystemContextMenuProvider : IContextMenuProvider<FileInfo>
    {
    }
}