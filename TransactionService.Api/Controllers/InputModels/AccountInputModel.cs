using System.ComponentModel.DataAnnotations;

namespace TransactionService.Api.Controllers.InputModels
{
    public class AccountInputModel
    {
        public string Externalid { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string CountryCode { get; set; }
        [Required] public string CurrencyCode { get; set; }
        [Required] public int StartingBalance { get; set; }
    }
}