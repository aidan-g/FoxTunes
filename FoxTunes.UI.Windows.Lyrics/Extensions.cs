using System.Diagnostics;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static Task WaitForExitAsync(this Process process)
        {
            var source = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => source.TrySetResult(null);
            return source.Task;
        }
    }
}
