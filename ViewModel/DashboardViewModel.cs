namespace Furni.ViewModel
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CanceledOrders { get; set; }

        public double PendingOrdersPercentage { get; set; }
        public double CompletedOrdersPercentage { get; set; }
        public double CanceledOrdersPercentage { get; set; }
    }
}