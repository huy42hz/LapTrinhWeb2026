using SV22T1020158.DataLayers.Interfaces;
using SV22T1020158.DataLayers.SQLServer;
using SV22T1020158.Models.Common;
using SV22T1020158.Models.Sales;

namespace SV22T1020158.BusinessLayers
{
    public static class SalesDataService
    {
        private static readonly IOrderRepository orderDB;

        static SalesDataService()
        {
            orderDB = new OrderRepository(Configuration.ConnectionString);
        }

        #region Order

        public static async Task<PagedResult<OrderViewInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            return await orderDB.ListAsync(input);
        }

        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            return await orderDB.GetAsync(orderID);
        }

        public static async Task<int> AddOrderAsync(int? customerID, string deliveryProvince, string deliveryAddress, IEnumerable<OrderDetailViewInfo> cartItems)
        {
            if (cartItems == null || !cartItems.Any()) return 0;
            if (customerID == null || customerID <= 0) return 0;
            if (string.IsNullOrWhiteSpace(deliveryProvince)) return 0;
            if (string.IsNullOrWhiteSpace(deliveryAddress)) return 0;

            var order = new Order
            {
                CustomerID = customerID,
                DeliveryProvince = deliveryProvince ?? "",
                DeliveryAddress = deliveryAddress ?? "",
                OrderTime = DateTime.Now,
                Status = OrderStatusEnum.New,
                EmployeeID = null,
                AcceptTime = null,
                ShipperID = null,
                ShippedTime = null,
                FinishedTime = null
            };

            return await orderDB.AddOrderAsync(order, cartItems);
        }
        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            if (data.OrderID <= 0) return false;

            var existingOrder = await orderDB.GetAsync(data.OrderID);
            if (existingOrder == null) return false;

            if (existingOrder.Status != OrderStatusEnum.New && existingOrder.Status != OrderStatusEnum.Accepted)
            {
                return false;
            }

            return await orderDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            if (orderID <= 0) return false;

            var existingOrder = await orderDB.GetAsync(orderID);
            if (existingOrder == null) return false;

            if (existingOrder.Status == OrderStatusEnum.Accepted ||
                existingOrder.Status == OrderStatusEnum.Shipping ||
                existingOrder.Status == OrderStatusEnum.Completed)
            {
                return false;
            }

            return await orderDB.DeleteAsync(orderID);
        }

        #endregion

        #region Order Status Processing

        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New)
                return false;

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.Status = OrderStatusEnum.Accepted;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> RejectOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New)
                return false;

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Rejected;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> CancelOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted &&
                order.Status != OrderStatusEnum.Shipping)
                return false;

            if (order.EmployeeID == null)
            {
                order.EmployeeID = employeeID;
                order.AcceptTime = DateTime.Now;
            }

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Cancelled;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Accepted)
                return false;

            order.ShipperID = shipperID;
            order.ShippedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Shipping;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Shipping)
                return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Completed;

            return await orderDB.UpdateAsync(order);
        }

        #endregion

        #region Order Detail

        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            return await orderDB.ListDetailsAsync(orderID);
        }

        public static async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            return await orderDB.GetDetailAsync(orderID, productID);
        }

        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            if (data.OrderID <= 0 || data.ProductID <= 0) return false;
            if (data.Quantity <= 0) return false;
            if (data.SalePrice < 0) return false;

            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
            {
                return false;
            }

            return await orderDB.AddDetailAsync(data);
        }

        public static async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            if (data.OrderID <= 0 || data.ProductID <= 0) return false;
            if (data.Quantity <= 0) return false;
            if (data.SalePrice < 0) return false;

            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
            {
                return false;
            }

            return await orderDB.UpdateDetailAsync(data);
        }

        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            if (orderID <= 0 || productID <= 0) return false;

            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
            {
                return false;
            }

            return await orderDB.DeleteDetailAsync(orderID, productID);
        }

        #endregion

        // Thêm phương thức này vào class SalesDataService
        /// <summary>
        /// Lấy danh sách đơn hàng của một khách hàng cụ thể
        /// </summary>
        public static async Task<PagedResult<OrderViewInfo>> ListOrdersByCustomerAsync(int customerId, OrderSearchInput input)
        {
            return await orderDB.ListByCustomerAsync(customerId, input);
        }
    }
}