using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace HackerNewsScraper.ConsoleApp
{
	public static class Helpers
	{
		public static string Serialize(List<Post> toReturn) =>
			JsonSerializer.Serialize<IEnumerable<Post>>(
				toReturn,
				new JsonSerializerOptions
				{
					IgnoreNullValues = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					WriteIndented = true,
				});

		public static bool IsValidUri(string? address) =>
			!string.IsNullOrWhiteSpace(address) &&
			(address.StartsWith("http://") || address.StartsWith("https://")) &&
			Uri.TryCreate(address, UriKind.Absolute, out var uri) &&
			Dns.GetHostAddresses(uri.DnsSafeHost).Length > 0;

	}
}
