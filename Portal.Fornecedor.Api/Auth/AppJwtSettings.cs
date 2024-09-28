namespace Portal.Fornecedor.Api.Auth
{
    public class AppJwtSettings
    {
        public string SecretKey { get; set; }
        public int Expiration { get; set; } = 24;//horas
        public string Issuer { get; set; } = "Portal.Fornecedor.Api";
        public string Audience { get; set; } = "Api";
    }
}
