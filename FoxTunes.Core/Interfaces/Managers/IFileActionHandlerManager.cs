using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IFileActionHandlerManager : IStandardManager
    {
        void RunCommand(string command);

        Task RunPaths(IEnumerable<string> paths);
    }
}
