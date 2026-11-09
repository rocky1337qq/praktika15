class EventAggregator
{
    private Dictionary<Type, List<Delegate>> handlers =
        new Dictionary<Type, List<Delegate>>();

    private readonly object locker = new object();

    public void Subscribe<T>(Action<T> handler)
    {
        lock (locker)
        {
            Type type = typeof(T);

            if (!handlers.ContainsKey(type))
            {
                handlers[type] = new List<Delegate>();
            }

            handlers[type].Add(handler);
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        lock (locker)
        {
            Type type = typeof(T);

            if (handlers.ContainsKey(type))
            {
                handlers[type].Remove(handler);

                if (handlers[type].Count == 0)
                {
                    handlers.Remove(type);
                }
            }
        }
    }

    public void Publish<T>(T message)
    {
        List<Delegate> subscribers;

        lock (locker)
        {
            if (!handlers.ContainsKey(typeof(T)))
            {
                return;
            }

            subscribers = new List<Delegate>(handlers[typeof(T)]);
        }

        foreach (Delegate handler in subscribers)
        {
            ((Action<T>)handler)(message);
        }
    }
}

class OrderPlaced
{
    public int Id { get; set; }
    public decimal Amount { get; set; }

    public OrderPlaced(int id, decimal amount)
    {
        Id = id;
        Amount = amount;
    }
}

class TemperatureChanged
{
    public double Temperature { get; set; }

    public TemperatureChanged(double temperature)
    {
        Temperature = temperature;
    }
}

class EmailNotifier
{
    public void Send(OrderPlaced order)
    {
        Console.WriteLine("Уведомление: заказ " + order.Id);
    }
}

class WarehouseService
{
    public void Process(OrderPlaced order)
    {
        Console.WriteLine("Склад получил заказ " + order.Id);
    }
}

class Alarm
{
    public void Check(TemperatureChanged message)
    {
        Console.WriteLine("Температура: " + message.Temperature);

        if (message.Temperature > 30)
        {
            Console.WriteLine("Внимание! Высокая температура!");
        }
    }
}

class Program
{
    static void Main()
    {
        EventAggregator aggregator = new EventAggregator();

        EmailNotifier email = new EmailNotifier();
        WarehouseService warehouse = new WarehouseService();
        Alarm alarm = new Alarm();

        aggregator.Subscribe<OrderPlaced>(email.Send);
        aggregator.Subscribe<OrderPlaced>(warehouse.Process);
        aggregator.Subscribe<TemperatureChanged>(alarm.Check);

        aggregator.Publish(new OrderPlaced(1, 5000));
        aggregator.Publish(new TemperatureChanged(35));

        aggregator.Unsubscribe<OrderPlaced>(warehouse.Process);

        Console.WriteLine("\nПосле отписки:");

        aggregator.Publish(new OrderPlaced(2, 7000));

        int counter = 0;

        aggregator.Subscribe<OrderPlaced>(order =>
        {
            Interlocked.Increment(ref counter);
        });

        Parallel.For(0, 1000, i =>
        {
            aggregator.Publish(new OrderPlaced(i, 100));
        });

        Console.WriteLine("\nКоличество обработанных сообщений: " + counter);
    }
}
