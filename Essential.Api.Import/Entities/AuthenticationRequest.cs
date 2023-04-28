namespace Essential.Api.Import.Entities
{
	public class AuthenticationRequest
	{
		public string grantType { get; set; } = "password";
		public string username { get; set; } = "tlapp@quantaservices.com";
		public string password { get; set; } = "Meridian!1";

		public AuthenticationRequest() { }
	}
}
