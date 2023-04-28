using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Vml.Spreadsheet;
using System;
using System.Net.Http.Headers;
using System.Text;

namespace Essential.Api.Import
{
	public class ApiClient : IDisposable
	{

		private HttpClient _client { get; set; }

		public ApiClient() { }			

		public ApiClient(string baseUrl)
		{
			_client = new HttpClient
			{
				BaseAddress = new Uri(baseUrl)
			};

			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public void AddHeader(string key, string value)
		{
			_client.DefaultRequestHeaders.Add(key, value);
		}

		public void RemoveHeader(string key)
		{
			_client.DefaultRequestHeaders.Remove(key);
		}

		public async Task<string> Get(string apiPath)
		{
			HttpResponseMessage responseMessage = await _client.GetAsync(apiPath);

			if (responseMessage.IsSuccessStatusCode)
			{
				return await responseMessage.Content.ReadAsStringAsync();
			}
			else
			{
				throw new Exception($"Error: {responseMessage.StatusCode} - {responseMessage.ReasonPhrase}");
			}
		}

		public async Task<string> Post(string apiPath, object body)
		{
			var serializationSettings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};

			var content = new StringContent(JsonConvert.SerializeObject(body,serializationSettings), Encoding.UTF8, "application/json");
			var responseMessage = await _client.PostAsync(apiPath, content);

			if (responseMessage.IsSuccessStatusCode)
			{
				return await responseMessage.Content.ReadAsStringAsync();
			}
			else
			{
				throw new Exception($"Error: {responseMessage.StatusCode} - {responseMessage.ReasonPhrase}");
			}
		}

		public void Dispose()
		{
			((IDisposable)_client).Dispose();
		}
	}
}
