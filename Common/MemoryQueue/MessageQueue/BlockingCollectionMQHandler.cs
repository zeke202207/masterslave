using System.Collections.Concurrent;

namespace NetX.MemoryQueue;

internal class BlockingCollectionMQHandler<TMessage> : MessageQueueHandler
    where TMessage : MessageArgument
{
    private static object _queueLock = new object();
    private BlockingCollection<TMessage> _queue;
    private PriorityQueue<TMessage, int> _priorityQueue;
    public Action<MessageArgument> _received;

    public BlockingCollectionMQHandler()
    {
        _queue = new BlockingCollection<TMessage>();
        _priorityQueue = new PriorityQueue<TMessage, int>();
    }

    public override bool IsBindReceivedEvent
    {
        get => this._received != null;
    }

    public override void BindReceivedEvent(Action<MessageArgument> received)
    {
        this._received = received;
        if (null != _received)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var message in _queue.GetConsumingEnumerable())
                {
                    if (!_queue.IsAddingCompleted)
                    {
                        try
                        {
                            if (null != message && null != received)
                            {
                                lock (_queueLock)
                                {
                                    _priorityQueue.Enqueue(message, message.Priority);
                                }
                                //_received.Invoke(message);
                            }
                        }
                        catch (Exception _)
                        {
                            //Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        if (null != received)
                            received = null;
                    }
                }
            });

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    TMessage message = null;
                    lock (_queueLock)
                    {
                        if (null == received ||
                        _priorityQueue.Count == 0 ||
                        !_priorityQueue.TryDequeue(out message, out int priority) ||
                        null == message)
                            continue;
                    }
                    received.Invoke(message);
                }
            });
        }
    }

    public override async Task<bool> Publish(MessageArgument message)
    {
        return await Task.Run<bool>(() =>
        {
            if (_queue.IsCompleted)
                return true;
            _queue.Add((TMessage)message);
            return true;
        });
    }
}
