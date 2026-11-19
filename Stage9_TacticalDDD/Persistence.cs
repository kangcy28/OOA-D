namespace Stage9_TacticalDDD;

// =========================================================================
// 📜 領域層介面契約（零 SQL、零資料庫依賴）
// =========================================================================

public interface IOrderRepository
{
    Order? GetById(Guid id);
    void Add(Order order);
}

public interface IProductRepository
{
    Product? GetById(Guid id);
    void Update(Product product);
}

public interface IUnitOfWork
{
    void Commit();
    void Rollback();
}

// =========================================================================
// 🗄️ 基礎設施層：In-Memory 模擬資料庫與交易工作單元
// =========================================================================

public class InMemoryDatabase
{
    public Dictionary<Guid, Order> Orders { get; } = new();
    public Dictionary<Guid, Product> Products { get; } = new();
}

public class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly List<Action> _pendingActions = new();

    internal void RegisterAction(Action action)
    {
        _pendingActions.Add(action);
    }

    public void Commit()
    {
        try
        {
            foreach (var action in _pendingActions)
            {
                action();
            }
        }
        finally
        {
            _pendingActions.Clear();
        }
    }

    public void Rollback()
    {
        _pendingActions.Clear();
    }
}

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly InMemoryDatabase _db;
    private readonly InMemoryUnitOfWork _uow;

    public InMemoryOrderRepository(InMemoryDatabase db, InMemoryUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public Order? GetById(Guid id) =>
        _db.Orders.TryGetValue(id, out var order) ? order : null;

    public void Add(Order order)
    {
        _uow.RegisterAction(() => _db.Orders[order.Id] = order);
    }
}

public class InMemoryProductRepository : IProductRepository
{
    private readonly InMemoryDatabase _db;
    private readonly InMemoryUnitOfWork _uow;

    public InMemoryProductRepository(InMemoryDatabase db, InMemoryUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public Product? GetById(Guid id) =>
        _db.Products.TryGetValue(id, out var product) ? product.Clone() : null;

    public void Update(Product product)
    {
        var snapshot = product.Clone();
        _uow.RegisterAction(() => _db.Products[snapshot.Id] = snapshot);
    }
}
