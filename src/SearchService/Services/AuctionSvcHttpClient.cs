using System;
using System.Net.Http.Json; // Add this namespace
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
    {
        this._config = config;
        this._httpClient = httpClient;

    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(a => a.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

   

        string url = _config["AuctionServiceUrl"] + "/api/auctions";
        if (lastUpdated != null)
        {
            url += "?date=" + lastUpdated.ToString();
        }
        return await _httpClient.GetFromJsonAsync<List<Item>>(url);
    }


}
