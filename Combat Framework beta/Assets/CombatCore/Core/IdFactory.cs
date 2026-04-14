using System.Threading;

namespace CombatCore.Core
{
    public static class IdFactory
    {
        private static long _baseId = 0;

        public static long NewInstanceId()
        {
            return Interlocked.Increment(ref _baseId);
        }
    }
}