﻿using Newtonsoft.Json;
using OpenDirectoryDownloader.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenDirectoryDownloader.FileUpload;

public class Pixeldrain : IFileUploadSite
{
	public string Name => "Pixeldrain.com";

	public async Task<IFileUploadSiteFile> UploadFile(HttpClient httpClient, string path)
	{
		int retries = 0;
		int maxRetries = 5;

		while (retries < maxRetries)
		{
			try
			{
				using (HttpResponseMessage httpResponseMessage = await httpClient.PutAsync($"https://pixeldrain.com/api/file/{Uri.EscapeDataString(Path.GetFileName(path))}", new StreamContent(new FileStream(path, FileMode.Open))))
				{
					if (httpResponseMessage.IsSuccessStatusCode)
					{
						string response = await httpResponseMessage.Content.ReadAsStringAsync();
						OpenDirectoryIndexer.Session.UploadedUrlsResponse = response;
						Program.Logger.Debug("Response from {siteName}: {response}", Name, response);

						return JsonConvert.DeserializeObject<PixeldrainFile>(response);
					}
					else
					{
						Program.Logger.Error("Error uploading file, retry in 5 seconds..");
						await Task.Delay(TimeSpan.FromSeconds(5));
					}
				}

				retries++;
			}
			catch (Exception ex)
			{
				retries++;
				Program.Logger.Error(ex, "Error uploading file, retry in 5 seconds..");
				await Task.Delay(TimeSpan.FromSeconds(5));
			}
		}

		throw new FriendlyException("Error uploading URLs");
	}
}

public class PixeldrainFile : IFileUploadSiteFile
{
	public string Url => $"https://pixeldrain.com/u/{Id}";

	[JsonProperty("id")]
	public string Id { get; set; }
}
