namespace LinqToDB.EntityFrameworkCore.BaseTests.Models.Northwind
{
    public class EmployeeTerritory : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string TerritoryId { get; set; }

        public Employee Employee { get; set; }
        public Territory Territory { get; set; }
    }
}
