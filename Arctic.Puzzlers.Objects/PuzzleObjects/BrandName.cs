using System.ComponentModel.DataAnnotations;

namespace Arctic.Puzzlers.Objects.PuzzleObjects
{
    public enum BrandName
    {
        Ravensburger,
        Schmidt,
        Clementoni,
        [Display(Name = "Buffalo Games")]
        BuffaloGames,
        Aquarius,
        Blanc,
        Trefl,
        Galison,
        Dino,
        Castorland,
        Masterpieces,
        Ceaco,
        [Display(Name = "White Mountain")]
        WhiteMountain,
        Eurographics,
        [Display(Name = "Bits and Pieces")]
        BitsandPieces,
        [Display(Name = "Cobble Hill")]
        CobbleHill,
        EeBoo,
        Holdson,
        Springbok,
        Mudpuppy,
        Jumbo,
        Unknown,
        Educa
    }
}