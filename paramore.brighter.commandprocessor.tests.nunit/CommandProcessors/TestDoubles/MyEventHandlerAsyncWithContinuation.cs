﻿using System.Threading;
using System.Threading.Tasks;

namespace paramore.brighter.commandprocessor.tests.nunit.CommandProcessors.TestDoubles
{
    class MyEventHandlerAsyncWithContinuation : RequestHandlerAsync<MyEvent>
    {
        private static MyEvent s_receivedEvent;
        private int counter = 0;

        public static ThreadLocal<int> LoopCounter { get; set; }
        public static int WorkThreadId { get; set; }
        public static int ContinuationThreadId { get; set; }

        public MyEventHandlerAsyncWithContinuation()
        {
            s_receivedEvent = null;
        }

        public override async Task<MyEvent> HandleAsync(MyEvent command, CancellationToken cancellationToken = default(CancellationToken))
        {
            LoopCounter = new ThreadLocal<int>(() => 0);
            WorkThreadId = 0;
            ContinuationThreadId = 0;
            LoopCounter.Value = 1;

            if (cancellationToken.IsCancellationRequested)
            {
                return command;
            }

            WorkThreadId = Thread.CurrentThread.ManagedThreadId;

            LogEvent(command);
            await Task.Delay(30, cancellationToken).ConfigureAwait(ContinueOnCapturedContext);

            //this will run on the continuation
            LoopCounter.Value = 2;
            ContinuationThreadId = Thread.CurrentThread.ManagedThreadId;

            return command;
        }

        private static void LogEvent(MyEvent @event)
        {
            s_receivedEvent = @event;
        }

        public static bool ShouldReceive(MyEvent myEvent)
        {
            return s_receivedEvent.Id == myEvent.Id;
        }
    }
}
