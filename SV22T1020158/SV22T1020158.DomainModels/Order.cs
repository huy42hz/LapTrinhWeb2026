namespace SV22T1020158.DomainModels
{
    public class Order
    {
        public int OrderID { get; set; }
        public int? CustomerID { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.Now;
        public string? DeliveryProvince { get; set; } = "";
        public string? DeliveryAddress { get; set; } = "";
        public int? EmployeeID { get; set; }
        public DateTime? AcceptTime { get; set; }
        public int? ShipperID { get; set; }
        public DateTime? ShippedTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public int Status { get; set; } = 1; // 1 = Mới tạo, 2 = Chấp nhận, 3 = Đang giao, 4 = Hoàn tất, -1 = Hủy, -2 = Từ chối,...

        // Để hiển thị trong view (không lưu DB)
        public string? CustomerName { get; set; }
        public string? EmployeeName { get; set; }
        public string? ShipperName { get; set; }
        public string? StatusDescription { get; set; }
    }

    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string? Photo { get; set; }
        public string Unit { get; set; } = "";
        public decimal SalePrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => SalePrice * Quantity;
    }

    public class OrderDetailForDisplay : OrderDetail
    {
        public string ProductName { get; set; } = "";
        public string? Photo { get; set; }
        public string Unit { get; set; } = "";
    }
}