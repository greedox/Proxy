using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Proxy.Services
{
    public abstract class AbstractActor<T>
    {
        /// <summary>
        /// Message queue
        /// </summary>
        private readonly BufferBlock<T> _mailBox;

        public int QueueCount => _mailBox.Count;

        /// <summary>
        /// Count of threads
        /// </summary>
        public abstract int ThreadCount { get; }

        public AbstractActor()
        {
            _mailBox = new BufferBlock<T>();

            var workers = new List<Task>();

            Task.Run(async () =>
            {
                while (true)
                {
                    while (workers.Count < ThreadCount)
                    {
                        workers.Add(Handle());
                    }

                    await Task.WhenAny(workers);
                    workers.RemoveAll(s => s.IsCompleted);
                }
            });

        }

        private async Task Handle()
        {
            var message = await _mailBox.ReceiveAsync();

            try
            {
                await HandleMessage(message);
            }
            catch (Exception ex)
            {
                await HandleError(message, ex);
            }
        }

        public abstract Task HandleMessage(T message);
        public abstract Task HandleError(T message, Exception ex);

        /// <summary>
        /// Send message to queue
        /// </summary>
        /// <param name="message"></param>
        public Task SendAsync(T message) => _mailBox.SendAsync(message);

        /// <summary>
        /// Stop message handling. The message queue will be cleared
        /// </summary>
        public void Stop() => _mailBox.TryReceiveAll(out var _);
    }

}
