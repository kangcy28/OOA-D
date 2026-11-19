namespace Stage9_TacticalDDD;

// =========================================================================
// 🚀 應用層服務（Use Case 協調者）
// =========================================================================

public class PlaceOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderService(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _unitOfWork = unitOfWork;
    }

    public Guid PlaceOrder(Guid customerId, Guid productId, int quantity)
    {
        try
        {
            Product? product = _productRepo.GetById(productId);
            if (product == null) throw new InvalidOperationException("商品不存在");

            product.DeductStock(quantity);
            _productRepo.Update(product);

            var order = new Order(customerId);
            order.AddItem(product.Id, product.Name, product.Price, quantity);
            _orderRepo.Add(order);

            _unitOfWork.Commit();
            return order.Id;
        }
        catch
        {
            _unitOfWork.Rollback();
            throw;
        }
    }
}
