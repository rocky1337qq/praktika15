class LongLivedPublisher
{
    public event EventHandler Tick;

    public void Raise()
    {
        Tick?.Invoke(this, EventArgs.Empty);
    }
}

class Subscriber
{
    public void OnTick(object sender, EventArgs e)
    {
        Console.WriteLine("Обычный подписчик получил событие.");
    }
}

class DisposableSubscriber : IDisposable
{
    private LongLivedPublisher publisher;

    public DisposableSubscriber(LongLivedPublisher publisher)
    {
        this.publisher = publisher;
        publisher.Tick += OnTick;
    }

    public void OnTick(object sender, EventArgs e)
    {
        Console.WriteLine("Подписчик с IDisposable получил событие.");
    }

    public void Dispose()
    {
        publisher.Tick -= OnTick;
        Console.WriteLine("Подписка отменена.");
    }
}

class Program
{
    static LongLivedPublisher publisher = new LongLivedPublisher();

    static WeakReference CreateLeakingSubscriber()
    {
        Subscriber subscriber = new Subscriber();

        publisher.Tick += subscriber.OnTick;

        return new WeakReference(subscriber);
    }

    static WeakReference CreateDisposableSubscriber()
    {
        DisposableSubscriber subscriber =
            new DisposableSubscriber(publisher);

        WeakReference reference = new WeakReference(subscriber);

        subscriber.Dispose();

        return reference;
    }

    static void CollectGarbage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    static void Main()
    {
        Console.WriteLine("Проверка утечки памяти\n");

        WeakReference leaking =
            CreateLeakingSubscriber();

        WeakReference normal =
            CreateDisposableSubscriber();

        CollectGarbage();

        Console.WriteLine("Обычный подписчик находится в памяти: "
            + leaking.IsAlive);

        Console.WriteLine("Подписчик после Dispose находится в памяти: "
            + normal.IsAlive);

        Console.WriteLine("\nВызов события:");

        publisher.Raise();
    }
}
