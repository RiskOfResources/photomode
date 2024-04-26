using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PhotoMode;

public class WebService {
   private Dictionary<string, HttpHandler> _settingsHandlers = new();
   private readonly PhotoModeSettings _settings;

   public WebService(PhotoModeSettings settings) {
      _settings = settings;
   }

   public async void Startup() {
      // shutdown previous instance
      if (_negotiatedPort > 0) {
         Shutdown();
      }
 
      _listener = new HttpListener();
      bool started = false;
      int port = 49152; // rfc6335 - ephemeral ports

      for (; port < 65536 && !started; port++) {
         try {
            _listener.Prefixes.Add($"http://*:{port}/");
            _listener.Start();
            started = true;

            Logger.Log($"Listening on port {port}");
            _negotiatedPort = port;
            break;
         }
         catch (Exception e) {
            _listener.Prefixes.Clear();
            Logger.Log(e);
         }
      }

      if (!started) {
         Logger.Log("Failed to bind any port for server");
         return;
      }

      AddSettingsHandlers();

      do {
         HttpListenerContext context = await _listener.GetContextAsync();
         HttpListenerRequest request = context.Request;
         HttpListenerResponse response = context.Response;
         using (response.OutputStream)
            Handle(request, response);
      } while (_listener.IsListening);
   }

   public void Shutdown() {
      Logger.Log($"Shutting down server, saving settings");
      foreach (var setting in _settings.Settings) {
         setting.Save();
      }

      _listener?.Close();
      _negotiatedPort = 0;
   }

   private delegate void HttpHandler(HttpListenerRequest request, HttpListenerResponse response);

   private void Handle(HttpListenerRequest request, HttpListenerResponse response) {
      if (request.HttpMethod == HttpMethod.Options.Method) {
         response.OutputStream.Close();
         return;
      }

      var resource = request.Url.Segments[request.Url.Segments.Length - 1];
      resource = HttpUtility.UrlDecode(resource);
      var handler = GetHandlerForResource(resource);

      try {
         handler(request, response);
      }
      catch (Exception e) {
         Logger.Log(e);
         response.StatusCode = (int)HttpStatusCode.InternalServerError;
      }
   }

   private void AddSettingsHandlers() {
      var photoSettings = _settings.Settings;

      foreach (var photoSetting in photoSettings) {
         _settingsHandlers[photoSetting.Name] = (request, response) => {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var body = reader.ReadToEnd();
            var update = JsonConvert.DeserializeObject<SettingUpdate<object>>(body);

            try {
               Logger.Log($"Updating setting {photoSetting.Name} with value: {update.Value}");
               photoSetting.UpdateValue(update.Value);
            }
            catch (Exception e) {
               Logger.Log($"Error updating value {e}");
               using var writer = new StreamWriter(response.OutputStream);
               writer.WriteLine("Couldn't update value");
               response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
         };
      }
   }

   private HttpHandler GetHandlerForResource(string resource) {
      if(_settingsHandlers.TryGetValue(resource, out var handler)) {
         return handler;
      }

      return resource switch {
         "settings" => HandleGetSettings,
         "save" => (_, _) => {
            foreach (var setting in _settings.Settings) {
               setting.Save();
            }
         },
         "ping" => (_, response) => {
            byte[] buffer = Encoding.UTF8.GetBytes("pong");
            response.OutputStream.Write(buffer, 0, buffer.Length);
         },
         _ => HandleServeFile
      };
   }

   private void HandleServeFile(HttpListenerRequest request, HttpListenerResponse response) {
      var baseDir = Path.GetDirectoryName(_settings.BaseDir);
      if (string.IsNullOrEmpty(baseDir)) {
         return;
      }

      string filePath = Path.Combine(baseDir, request.Url.AbsolutePath.TrimStart('/'));
      if (File.Exists(filePath))
      {
         byte[] buffer = File.ReadAllBytes(filePath);
         response.ContentLength64 = buffer.Length;
         if (filePath.EndsWith(".js")) {
            response.ContentType = "application/javascript";
         }
         using var output = response.OutputStream;
         output.Write(buffer, 0, buffer.Length);
      }
      else
      {
         response.StatusCode = 404;
      }
   }

   private void HandleGetSettings(HttpListenerRequest request, HttpListenerResponse response) {
      var serialize = JsonConvert.SerializeObject(_settings, new StringEnumConverter());
      using var writer = new StreamWriter(response.OutputStream);
      writer.WriteLine(serialize);
   }

   public int NegotiatedPort => _negotiatedPort;
   private int _negotiatedPort;
   private HttpListener _listener;

   public class SettingUpdate<T> {
      public T Value { get; set; }
   }
}