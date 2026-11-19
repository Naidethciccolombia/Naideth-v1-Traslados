using System.Text.Json.Serialization;

namespace Naideth.Traslados.Infrastructura.Civitatis.Models.Rq
{
    public class ConfirmarRq
    {
        [JsonPropertyName("walletKey")]
        public string WalletKey { get; set; } = string.Empty;
    }
}
