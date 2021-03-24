using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UniAgile.Unity
{
    public static class TaskEx
    {
        public static async Task RunUntilCancellation(this CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }

        public static async Task<T> RunUntilCancellation<T>(this CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();
            }

            return default;
        }

        public static async Task<T> RunUntilCancellation<T>(CancellationToken cancellationToken,
                                                            Func<T> selector)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();
            }

            return selector();
        }

        public static async Task<T> WaitWhileDefault<T>(Func<T> selector,
                                                        CancellationToken cancellationToken,
                                                        int frequency = 1,
                                                        int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
                                    {
                                        while (EqualityComparer<T>.Default.Equals(selector(), default))
                                        {
                                            await Task.Delay(frequency, cancellationToken);
                                        }
                                    },
                                    cancellationToken);

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout, cancellationToken)))
            {
                throw new TimeoutException();
            }

            return selector();
        }
        
        private readonly struct Awaiter<T>
        {
            private readonly T Obj;
            private readonly Func<T, bool> Condition;
            private readonly bool ResultToFail;

            public Awaiter(T obj,
                           Func<T, bool> condition, bool resultToPass)
            {
                Obj = obj;
                Condition = condition;
                ResultToFail = !resultToPass;
            }

            public async Task WaitUntilCondition()
            {
                while (ResultToFail == Condition(Obj))
                {
                    await Task.Yield();
                }
            }

            public async Task<T> WaitUntilTypedCondition()
            {
                while (!ResultToFail == Condition(Obj))
                {
                    await Task.Yield();
                }

                return Obj;
            }
        }
        
        public static async Task WaitUntil<T>(T obj, Func<T, bool> condition, bool resultToPass)
        {
            var awaiter = new Awaiter<T>(obj, condition, resultToPass);
            await awaiter.WaitUntilCondition();
        }
        
        public static async Task<T> WaitWhileReturn<T>(T obj, Func<T, bool> condition, bool resultToPass)
        {
            var awaiter = new Awaiter<T>(obj, condition, resultToPass);
            return await awaiter.WaitUntilTypedCondition();
        }
        
        
    }
}