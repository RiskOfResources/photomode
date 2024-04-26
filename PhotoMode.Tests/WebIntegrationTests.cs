using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace PhotoMode.Tests;

public class WebIntegrationTests : IDisposable {
   private readonly ITestOutputHelper _testOutputHelper;
   private readonly WebService _ws;
   private HttpClient _client;

   public WebIntegrationTests(ITestOutputHelper testOutputHelper) {
      _testOutputHelper = testOutputHelper;
      Logger.Log = message => testOutputHelper.WriteLine(message?.ToString());  
      _client = new HttpClient();
      _ws = new WebService(new PhotoModeSettings(null, AppDomain.CurrentDomain.BaseDirectory));
      _ws.Startup();
      testOutputHelper.WriteLine($"listen on {_ws.NegotiatedPort}");
   }

   public void Dispose() {
      _ws.Shutdown();
   }

   [Fact]
   public async Task ServerStarts() {
      var res = await _client.GetAsync($"http://localhost:{_ws.NegotiatedPort}/ping");
      Assert.Equal(HttpStatusCode.OK, res.StatusCode);
      var text = await res.Content.ReadAsStringAsync();
      Assert.Equal("pong", text);
   }

   [Fact]
   public async void FocusDistanceUpdates() {
      var settings = await _getSettings();
      Assert.Equal(10, settings.PostProcessFocusDistance.Value);
      var settingUpdate = new WebService.SettingUpdate<float>() { Value = 50f };
      await _client.PostAsJsonAsync($"http://localhost:{_ws.NegotiatedPort}/Focus Distance", settingUpdate);
      var newSettings = await _getSettings();
      Assert.Equal(50, newSettings.PostProcessFocusDistance.Value);
   }

   [Fact]
   public async Task RetrieveSettings() {
      var settings = await _getSettings();
      Assert.NotNull(settings);
      var sens = settings.CameraSensitivity;
      Assert.Equal(1, sens.Value);
   }

   private async Task<PhotoModeSettings> _getSettings() {
      var settings = await _client.GetFromJsonAsync<PhotoModeSettings>($"http://localhost:{_ws.NegotiatedPort}/settings");
      return settings;
   }

   [Fact(Skip = "Convenience test for dev")]
   // [Fact]
   public void RunServer() {
      Console.ReadLine();
   }
}
