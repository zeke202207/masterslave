
namespace NetX.Master;

public interface IJobPublisher
{
    void Subscribe(IObserver<WorkerJob> observer);
    void Unsubscribe(IObserver<WorkerJob> observer);
    void Publish(WorkerJob job);
}
