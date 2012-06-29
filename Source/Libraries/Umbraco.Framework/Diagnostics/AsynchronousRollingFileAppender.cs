namespace Umbraco.Framework.Diagnostics
{
    #region Imports

    using System;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Util;

    #endregion

    /// <summary>
    /// Based on code by Chris Haines http://cjbhaines.wordpress.com/2012/02/13/asynchronous-log4net-appenders/
    /// </summary>
    public class AsynchronousRollingFileAppender : RollingFileAppender
    {
        private readonly ManualResetEvent manualResetEvent;
        private int bufferOverflowCounter;
        private bool forceStop;
        private bool hasFinished;
        private DateTime lastLoggedBufferOverflow;
        private bool logBufferOverflow;
        private RingBuffer<LoggingEvent> pendingAppends;
        private int queueSizeLimit = 1000;
        private bool shuttingDown;

        public AsynchronousRollingFileAppender()
        {
            manualResetEvent = new ManualResetEvent(false);
        }

        public int QueueSizeLimit
        {
            get { return queueSizeLimit; }
            set { queueSizeLimit = value; }
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            pendingAppends = new RingBuffer<LoggingEvent>(QueueSizeLimit);
            pendingAppends.BufferOverflow += OnBufferOverflow;
            StartAppendTask();
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            Array.ForEach(loggingEvents, Append);
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (FilterEvent(loggingEvent))
            {
                pendingAppends.Enqueue(loggingEvent);
            }
        }

        protected override void OnClose()
        {
            shuttingDown = true;
            manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));

            if (!hasFinished)
            {
                forceStop = true;
                var windowsIdentity = WindowsIdentity.GetCurrent();
                base.Append(new LoggingEvent(new LoggingEventData
                    {
                        Level = Level.Error,
                        Message =
                            "Unable to clear out the AsynchronousRollingFileAppender buffer in the allotted time, forcing a shutdown",
                        TimeStamp = DateTime.UtcNow,
                        Identity = "",
                        ExceptionString = "",
                        UserName = windowsIdentity != null ? windowsIdentity.Name : "",
                        Domain = AppDomain.CurrentDomain.FriendlyName,
                        ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
                        LocationInfo =
                            new LocationInfo(this.GetType().Name, "OnClose", "AsynchronousRollingFileAppender.cs", "59"),
                        LoggerName = this.GetType().FullName,
                        Properties = new PropertiesDictionary(),
                    })
                    );
            }

            base.OnClose();
        }

        private void StartAppendTask()
        {
            if (!shuttingDown)
            {
                Task appendTask = new Task(AppendLoggingEvents, TaskCreationOptions.LongRunning);
                appendTask.LogErrors(LogAppenderError).ContinueWith(x => StartAppendTask()).LogErrors(LogAppenderError);
                appendTask.Start();
            }
        }

        private void LogAppenderError(string logMessage, Exception exception)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            base.Append(new LoggingEvent(new LoggingEventData
                {
                    Level = Level.Error,
                    Message = "Appender exception: " + logMessage,
                    TimeStamp = DateTime.UtcNow,
                    Identity = "",
                    ExceptionString = exception.ToString(),
                    UserName = windowsIdentity != null ? windowsIdentity.Name : "",
                    Domain = AppDomain.CurrentDomain.FriendlyName,
                    ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
                    LocationInfo =
                        new LocationInfo(this.GetType().Name,
                                         "LogAppenderError",
                                         "AsynchronousRollingFileAppender.cs",
                                         "100"),
                    LoggerName = this.GetType().FullName,
                    Properties = new PropertiesDictionary(),
                }));
        }

        private void AppendLoggingEvents()
        {
            LoggingEvent loggingEventToAppend;
            while (!shuttingDown)
            {
                if (logBufferOverflow)
                {
                    LogBufferOverflowError();
                    logBufferOverflow = false;
                    bufferOverflowCounter = 0;
                    lastLoggedBufferOverflow = DateTime.UtcNow;
                }

                while (!pendingAppends.TryDequeue(out loggingEventToAppend))
                {
                    Thread.Sleep(10);
                    if (shuttingDown)
                    {
                        break;
                    }
                }
                if (loggingEventToAppend == null)
                {
                    continue;
                }

                try
                {
                    base.Append(loggingEventToAppend);
                }
                catch
                {
                }
            }

            while (pendingAppends.TryDequeue(out loggingEventToAppend) && !forceStop)
            {
                try
                {
                    base.Append(loggingEventToAppend);
                }
                catch
                {
                }
            }
            hasFinished = true;
            manualResetEvent.Set();
        }

        private void LogBufferOverflowError()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            base.Append(new LoggingEvent(new LoggingEventData
                {
                    Level = Level.Error,
                    Message =
                        string.Format(
                            "Buffer overflow. {0} logging events have been lost in the last 30 seconds. [QueueSizeLimit: {1}]",
                            bufferOverflowCounter,
                            QueueSizeLimit),
                    TimeStamp = DateTime.UtcNow,
                    Identity = "",
                    ExceptionString = "",
                    UserName = windowsIdentity != null ? windowsIdentity.Name : "",
                    Domain = AppDomain.CurrentDomain.FriendlyName,
                    ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
                    LocationInfo =
                        new LocationInfo(this.GetType().Name,
                                         "LogBufferOverflowError",
                                         "AsynchronousRollingFileAppender.cs",
                                         "172"),
                    LoggerName = this.GetType().FullName,
                    Properties = new PropertiesDictionary(),
                }));
        }

        private void OnBufferOverflow(object sender, EventArgs eventArgs)
        {
            bufferOverflowCounter++;
            if (logBufferOverflow == false)
            {
                if (lastLoggedBufferOverflow < DateTime.UtcNow.AddSeconds(-30))
                {
                    logBufferOverflow = true;
                }
            }
        }
    }
}