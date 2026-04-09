using System.Threading;

namespace EGamePlay
{
    public static class IdFactory
    {
        public static long _baseId = 0;

        public static long NewInstanceId()
        {
            return Interlocked.Increment(ref _baseId);
        }
    }
}