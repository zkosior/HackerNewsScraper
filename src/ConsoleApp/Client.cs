using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace HackerNewsScraper.ConsoleApp
{
	[SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "Approved")]
	public sealed class Client : IDisposable
	{
		private readonly HttpClient client;
		private bool disposed;

		public Client(string baseAddress) =>
			this.client = new HttpClient
			{
				BaseAddress = new Uri(baseAddress),
			};

		public void Dispose()
		{
			if (!this.disposed)
			{
				this.client.Dispose();
				this.disposed = true;
			}
		}

		public async Task<string> DownloadContent(int page)
		{
			try
			{
				return await this.client.GetStringAsync($"news?p={page}");
			}
			catch (HttpRequestException)
			{
				// checking status code might work better here
				throw new ApplicationException("Could not download posts.");
			}
		}
	}
}