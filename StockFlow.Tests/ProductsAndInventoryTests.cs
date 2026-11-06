using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StockFlow.DTOs;
using StockFlow.Tests.Infrastructure;

namespace StockFlow.Tests;

public class ProductsAndInventoryTests
{
    private static StockFlowWebApplicationFactory CreateFactory()
        => new();

    private static async Task<int> CreateProductAsync(
        HttpClient client,
        int quantity = 20,
        int reorderLevel = 5)
    {
        var sku = $"TEST-{Guid.NewGuid():N}"[..13];

        var response = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                sku,
                name = "Test Wireless Mouse",
                description = "Created by integration tests",
                quantityInStock = quantity,
                reorderLevel,
                unitPrice = 199.99m
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var product = await response.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(product);

        return product.Id;
    }

    [Fact]
    public async Task GetProducts_ReturnsOkAndEmptyList()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Empty(document.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task GetMissingProduct_ReturnsNotFound()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreatedAndCanBeRetrieved()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var productId = await CreateProductAsync(client);

        var response = await client.GetAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(product);
        Assert.Equal(productId, product.Id);
        Assert.Equal("Test Wireless Mouse", product.Name);
        Assert.Equal(20, product.QuantityInStock);
    }

    [Fact]
    public async Task ReceivedMovement_IncreasesStock()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var productId = await CreateProductAsync(client, quantity: 20);

        var movementResponse = await client.PostAsJsonAsync(
            "/api/inventorymovements",
            new
            {
                productId,
                type = 0,
                quantity = 5,
                notes = "Integration test stock receipt"
            });

        Assert.True(
            movementResponse.IsSuccessStatusCode,
            $"Unexpected response: {await movementResponse.Content.ReadAsStringAsync()}");

        var productResponse = await client.GetAsync(
            $"/api/products/{productId}");

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(product);
        Assert.Equal(25, product.QuantityInStock);
    }

    [Fact]
    public async Task SoldMovement_CannotReduceStockBelowZero()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var productId = await CreateProductAsync(client, quantity: 2);

        var movementResponse = await client.PostAsJsonAsync(
            "/api/inventorymovements",
            new
            {
                productId,
                type = 1,
                quantity = 3,
                notes = "This sale should be rejected"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            movementResponse.StatusCode);

        var productResponse = await client.GetAsync(
            $"/api/products/{productId}");

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(product);
        Assert.Equal(2, product.QuantityInStock);
    }

    [Fact]
    public async Task LowStockEndpoint_ReturnsProductsBelowReorderLevel()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await CreateProductAsync(
            client,
            quantity: 2,
            reorderLevel: 5);

        var response = await client.GetAsync(
            "/api/products/low-stock");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        var products = document.RootElement.EnumerateArray();

        Assert.Contains(products, product =>
            product.GetProperty("name").GetString()
                == "Test Wireless Mouse");
    }
}