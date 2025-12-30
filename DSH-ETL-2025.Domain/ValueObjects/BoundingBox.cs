namespace DSH_ETL_2025.Domain.ValueObjects;

public class BoundingBox
{
    public decimal WestBoundLongitude { get; set; }

    public decimal EastBoundLongitude { get; set; }

    public decimal SouthBoundLatitude { get; set; }

    public decimal NorthBoundLatitude { get; set; }
}

