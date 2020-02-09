using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HackerNewsScraper.ConsoleApp
{
	public class Client
	{
		private readonly HttpClient client;

		public Client(string baseAddress) =>
			client = new HttpClient
			{
				BaseAddress = new Uri(baseAddress),
			};

		public async Task<string> DownloadContent(int page)
		{
			try
			{
				return await client.GetStringAsync($"/news?p={page}");
			}
			catch (HttpRequestException)
			{
				// checking status code might work better here
				throw new ApplicationException("Could not download posts.");
			}
		}
	}
}