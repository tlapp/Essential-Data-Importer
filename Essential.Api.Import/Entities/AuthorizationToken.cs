namespace Essential.Api.Import.Entities
{
	public class AuthorizationToken
	{
		public string bearerToken { get; set; }
		public string refreshToken { get; set; }
		public int expiresInMinutes { get; set; }
		public int refreshTokenExpiresInMinutes { get; set; }
	}
}
