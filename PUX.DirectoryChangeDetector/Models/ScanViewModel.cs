using System.ComponentModel.DataAnnotations;

namespace PUX.DirectoryChangeDetector.Models;

public sealed class ScanViewModel
{
    [Display(Name = "Cesta k adresáři")]
    [Required(ErrorMessage = "Zadejte cestu k analyzovanému adresáři.")]
    public string? DirectoryPath { get; set; }

    public ScanResult? Result { get; set; }
}
