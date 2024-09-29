namespace Portal.Fornecedor.Api.Models.User.Response
{
    public class UserResponse
    {
        public string AccessToken { get; set; }

        public bool HttpOnly { get; set; }

        public bool Secure { get; set; }

        public SameSiteMode SameSite { get; set; }

    }
}
