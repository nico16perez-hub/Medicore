using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public class ItemNutricionRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(30)]
    public string Dia { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(60)]
    public string Comida { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(240)]
    public string Detalle { get; set; } = string.Empty;
}
