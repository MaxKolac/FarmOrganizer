namespace FarmOrganizer.Models;

public partial class FieldEfficiency
{
    public int Id { get; set; }
    public int IdCropField { get; set; }
    public int IdSeason { get; set; }
    public double Hectares { get; set; }
    public double CropAmount { get; set; }

    public virtual CropField IdCropFieldNavigation { get; set; }
    public virtual Season IdSeasonNavigation { get; set; }
}
