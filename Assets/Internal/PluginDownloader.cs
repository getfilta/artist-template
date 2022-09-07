using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class PluginDownloader
{
    private const string RegistryName = "Filta Artist Suite";
    private const string RegistryUrl = "https://registry.npmjs.org";
    private const string PluginUrl = "https://registry.npmjs.org/com.getfilta.artist-unityplug";
    private const string RegistryScope = "com.getfilta.artist-unityplug";
    
    [InitializeOnLoadMethod]
    private static async void UpdatePanel() {
        ScopedRegistry filtaRegistry = new ScopedRegistry {
            name = RegistryName,
            url = RegistryUrl,
            scopes = new[] {
                RegistryScope
            }
        };
        string manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
        string manifestJson = File.ReadAllText(manifestPath);
        ManifestJson manifest = JsonConvert.DeserializeObject<ManifestJson>(manifestJson);
        string version;
        if (manifest.scopedRegistries.FindIndex((registry => registry.url == RegistryUrl)) == -1) {
            version = await GetVersion();
            if (String.IsNullOrEmpty(version)) {
                Retry();
                return;
            }
            manifest.scopedRegistries.Add(filtaRegistry);
        } else {
            return;
        }
        if (!manifest.dependencies.ContainsKey(RegistryScope)) {
            manifest.dependencies.Add(RegistryScope, version);
        } else {
            manifest.dependencies[RegistryScope] = version;
        }
        File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
        UnityEditor.PackageManager.Client.Resolve();
    }

    private static void Retry() {
        bool answer = EditorUtility.DisplayDialog("Failed to download plugin",
            "We failed to download the artist plugin. Ensure you have a stable internet connection", "Retry", "Quit");
        if (answer) {
            UpdatePanel();
        } else {
            EditorApplication.Exit(0);
        }
    }
    
    private static async Task<string> GetVersion() {
        try {
            using UnityWebRequest req = UnityWebRequest.Get(PluginUrl);
            await req.SendWebRequest();
            Debug.Log(req.downloadHandler.text);
            JObject jsonResult = JObject.Parse(req.downloadHandler.text);
            JToken versions = jsonResult["dist-tags"];
            if (versions == null) {
                Debug.LogError("Could not find dist-tags");
                return null;
            }

            JToken token = versions["latest"];
            if (token != null) {
                return token.Value<string>();
            }
        } catch (Exception e) {
            Debug.LogError(e.Message);
            return null;
        }

        return null;
    }
    
    public class ScopedRegistry {
        public string name;
        public string url;
        public string[] scopes;
    }
 
    public class ManifestJson {
        public Dictionary<string,string> dependencies = new Dictionary<string, string>();
 
        public List<ScopedRegistry> scopedRegistries = new List<ScopedRegistry>();
    }
}
public class UnityWebRequestAwaiter : INotifyCompletion {
    private readonly UnityWebRequestAsyncOperation asyncOp;
    private Action continuation;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp) {
        this.asyncOp = asyncOp;
        asyncOp.completed += OnRequestCompleted;
    }

    public bool IsCompleted => asyncOp.isDone;

    public void GetResult() { }

    public void OnCompleted(Action continuation) {
        this.continuation = continuation;
    }

    private void OnRequestCompleted(AsyncOperation obj) {
        continuation();
    }
}

public static class ExtensionMethods {
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp) {
        return new UnityWebRequestAwaiter(asyncOp);
    }
}
