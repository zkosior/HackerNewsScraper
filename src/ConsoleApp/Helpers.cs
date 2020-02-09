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

		public static bool ValidateAddress(string? address, out string validatedAddress)
		{
			validatedAddress = "https://news.ycombinator.com/";
			if (address == null)
			{
				return true;
			}
			else if (!IsValidUri(address))
			{
				Console.WriteLine("Not valid Uri.");
				return false;
			}
			else if (!address.EndsWith('/')) // already is valid
			{
				validatedAddress = address + "/"; // for consistency
				return true;
			}

			return false; // shouldn't happen
		}

		public static bool ValidatePostsCount(int posts, out int validatedPosts)
		{
			if (posts <= 0)
			{
				Console.WriteLine("Nothing to download.");
				validatedPosts = 0;
				return false;
			}

			if (posts > 100)
			{
				Console.WriteLine("Too much to download. Limiting to 100.");
				validatedPosts = 100;
				return true;
			}

			validatedPosts = posts;
			return true;
		}
	}
}
