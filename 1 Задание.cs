class OrderCancellingEventArgs : EventArgs
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerEmail { get; set; }
    public bool Cancel { get; set; }
    public string Reason { get; set; }

    public OrderCancellingEventArgs(int id, decimal amount, string email)
    {
        OrderId = id;
        Amount = amount;
        CustomerEmail = email;
        Cancel = false;
        Reason = "";
    }
}

class OrderPlacedEventArgs : EventArgs
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }

    public OrderPlacedEventArgs(int id, decimal amount)
    {
        OrderId = id;
        Amount = amount;
    }
}

class OrderService
{
    public event EventHandler<OrderCancellingEventArgs> OrderCancelling;
    public event EventHandler<OrderPlacedEventArgs> OrderPlaced;

    public void PlaceOrder(int id, decimal amount, string email)
    {
        Console.WriteLine("\nПопытка оформления заказа " + id);

        OrderCancellingEventArgs args =
            new OrderCancellingEventArgs(id, amount, email);

        if (OrderCancelling != null)
        {
            foreach (Delegate handler in OrderCancelling.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка обработчика: " + ex.Message);
                }

                if (args.Cancel)
                {
                    Console.WriteLine("Заказ отменён. Причина: " + args.Reason);
                    return;
                }
            }
        }

        Console.WriteLine("Заказ успешно оформлен!");

        OrderPlacedEventArgs placedArgs =
            new OrderPlacedEventArgs(id, amount);

        if (OrderPlaced != null)
        {
            foreach (Delegate handler in OrderPlaced.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, placedArgs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка обработчика: " + ex.Message);
                }
            }
        }
    }
}

class StockValidator
{
    public void Check(object sender, OrderCancellingEventArgs e)
    {
        if (e.Amount > 20000)
        {
            e.Cancel = true;
            e.Reason = "Сумма заказа превышает 20000";
        }
    }
}

class FraudDetector
{
    public void Check(object sender, OrderCancellingEventArgs e)
    {
        if (e.CustomerEmail.EndsWith(".test"))
        {
            e.Cancel = true;
            e.Reason = "Подозрительный адрес электронной почты";
        }
    }
}

class AuditLog
{
    public void Log(object sender, OrderCancellingEventArgs e)
    {
        Console.WriteLine("Проверка заказа " + e.OrderId);
    }
}

class EmailNotifier
{
    public void Send(object sender, OrderPlacedEventArgs e)
    {
        Console.WriteLine("Уведомление отправлено клиенту.");
    }
}

class WarehouseService
{
    public void Process(object sender, OrderPlacedEventArgs e)
    {
        Console.WriteLine("Заказ передан на склад.");
    }
}

class Program
{
    static void Main()
    {
        OrderService service = new OrderService();

        StockValidator stock = new StockValidator();
        FraudDetector fraud = new FraudDetector();
        AuditLog audit = new AuditLog();

        EmailNotifier email = new EmailNotifier();
        WarehouseService warehouse = new WarehouseService();

        service.OrderCancelling += stock.Check;
        service.OrderCancelling += fraud.Check;
        service.OrderCancelling += audit.Log;

        service.OrderPlaced += email.Send;
        service.OrderPlaced += warehouse.Process;

        service.PlaceOrder(1, 5000, "user@mail.com");
        service.PlaceOrder(2, 25000, "user@mail.com");
        service.PlaceOrder(3, 15000, "user@mail.test");
    }
}