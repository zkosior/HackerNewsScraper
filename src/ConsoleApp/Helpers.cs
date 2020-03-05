using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Encodings.Web;
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
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				});

		public static bool IsValidUri(string? address) =>
			!string.IsNullOrWhiteSpace(address) &&
			(address.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
			address.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
			Uri.TryCreate(address, UriKind.Absolute, out var uri) &&
			IsResolvable(uri);

		public static bool ValidateAddress(string? address, out string validatedAddress)
		{
			if (address == null)
			{
				validatedAddress = "https://news.ycombinator.com/";
				return true;
			}
			else if (!IsValidUri(address))
			{
				Console.WriteLine("Not valid Uri.");
				validatedAddress = string.Empty;
				return false;
			}
			else if (address.EndsWith('/'))
			{
				validatedAddress = address;
				return true;
			}
			else
			{
				validatedAddress = address + "/"; // for consistency
				return true;
			}
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

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Leave for now. Not a main focus of this exercise.")]
		private static bool IsResolvable(Uri uri)
		{
			try
			{
				return Dns.GetHostAddresses(uri.DnsSafeHost).Length > 0;
			}
			catch
			{
				return false;
			}
		}
	}
}
