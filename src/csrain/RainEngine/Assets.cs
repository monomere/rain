using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RainEngine
{
	public enum AssetType
	{
		Texture,
		StaticAudio,
		StreamAudio
	}

	public struct AssetID
	{
		public readonly ulong Raw;

		public AssetID(ulong raw) => Raw = raw;

		public static AssetID Empty => new(0);

		public override string ToString() => $"{{{Raw}}}";

		public T? Get<T>(AssetManager? manager = null)
		{
			if (manager == null) manager = AssetManager.Active;
			return manager.Get<T>(this);
		}
	}

	public class AssetBase
	{
		public readonly AssetID ID;
		public AssetBase(AssetID id) => ID = id;
	}

	public class Asset<T> : AssetBase
	{
		public Asset(AssetID id) : base(id) { }
		public Asset() : this(AssetID.Empty) { }
		public T? Get(AssetManager? manager = null) => ID.Get<T>(manager);
		public override string ToString() => $"<{typeof(T).Name}>{ID}";
	}

	public class AssetJsonConverter : JsonConverter<AssetBase>
	{
		public override bool CanConvert(Type typeToConvert) =>
			typeof(AssetBase).IsAssignableFrom(typeToConvert);

		public override AssetBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected object start.");

			reader.Read();
			if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected property name.");
			string? propertyName = reader.GetString();
			if (propertyName != "id") throw new JsonException("Expected 'id' property.");
			
			reader.Read();
			if (reader.TokenType != JsonTokenType.Number) throw new JsonException("Expected number value.");
			
			ulong id = reader.GetUInt64();

			reader.Read();
			if (reader.TokenType != JsonTokenType.EndObject) throw new JsonException("Expected object end.");

			return (AssetBase)Activator.CreateInstance(typeToConvert, new object[] {
				new AssetID(id)
			});
		}

		public override void Write(Utf8JsonWriter writer, AssetBase value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("id", value.ID.Raw);
			writer.WriteEndObject();
		}
	}

	public interface IAssetLoader
	{
		public (object, Type) Load(AssetID id, JsonElement data);
	}

	public class TextureAssetLoader : IAssetLoader
	{
		public (object, Type) Load(AssetID id, JsonElement data)
		{
			var path = data.GetProperty("path").GetString()!;
			var format = (TextureFormat)data.GetProperty("format").GetUInt32();
			return (Texture.FromFile(id, path, format), typeof(Texture));
		}
	}

	public class AudioAssetLoader : IAssetLoader
	{
		public (object, Type) Load(AssetID id, JsonElement data)
		{
			throw new NotImplementedException();
		}
	}

	public class AssetManager
	{
		public static Dictionary<AssetType, IAssetLoader> Loaders = new Dictionary<AssetType, IAssetLoader>()
		{
			{ AssetType.Texture, new TextureAssetLoader() },
			{ AssetType.StaticAudio, new AudioAssetLoader() },
			{ AssetType.StreamAudio, new AudioAssetLoader() },
		};

		public static AssetManager Active { get; set; } = new();

		public struct AssetInfo
		{
			public object Data;
			public string Name;
		}

		public Dictionary<ulong, AssetInfo> Assets = new();
		public Dictionary<string, ulong> AssetNames = new();

		public T? Get<T>(AssetID id)
		{
			if (id.Raw == 0) return default(T);
			if (Assets.ContainsKey(id.Raw)) return (T)Assets[id.Raw].Data;
			throw new Exception($"No such asset loaded: {id}.");
		}

		public T? Get<T>(string name)
		{
			if (AssetNames.ContainsKey(name)) return (T)Assets[AssetNames[name]].Data;
			throw new Exception($"No such asset loaded: '{name}'.");
		}

		public void LoadAllFromManifestJson(JsonElement manifest)
		{
			foreach (var assetJson in manifest.EnumerateArray())
			{
				var type = (AssetType)assetJson.GetProperty("type").GetUInt32();
				var id = new AssetID(assetJson.GetProperty("id").GetUInt64());
				var name = assetJson.GetProperty("name").GetString()!;
				if (Assets.ContainsKey(id.Raw))
				{
					throw new Exception($"Duplicate Asset ID: {id}");
				}
				var (obj, _) = Loaders[type].Load(id, assetJson);
				Assets[id.Raw] = new() { Data = obj, Name = name };
				AssetNames[name] = id.Raw;
			}
		}

		public void LoadAllFromManifestFile(string path)
		{
			var doc = JsonDocument.Parse(File.ReadAllText(path));
			LoadAllFromManifestJson(doc.RootElement);
		}
	}

	public static class ComponentAsset
	{
		public static Component BuildFromJson(JsonElement element)
		{
			var componentTypeName = element.GetProperty("type").GetString()!;
			var type = Type.GetType(componentTypeName);
			if (!type.IsSubclassOf(typeof(Component)))
				throw new Exception($"Type '{type}' is not a subclass of Component");
			return (Component)JsonSerializer.Deserialize(element.GetProperty("data"), type, new JsonSerializerOptions
			{
				ReferenceHandler = ReferenceHandler.Preserve,
				IncludeFields = true,
				PropertyNameCaseInsensitive = true,
				Converters = { new AssetJsonConverter() }
			})!;
		}

		public static void SaveToJson(Component component, Utf8JsonWriter doc)
		{
			doc.WriteStartObject();
			doc.WriteString("type", component.GetType().FullName);
			doc.WritePropertyName("data");
			JsonSerializer.Serialize(doc, component, component.GetType(), new JsonSerializerOptions
			{
				ReferenceHandler = ReferenceHandler.Preserve,
				IncludeFields = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Converters = { new AssetJsonConverter() }
			});
			doc.WriteEndObject();
		}
	}

	public static class EntityAsset
	{
		public static Entity BuildFromJson(JsonElement element, Scene scene)
		{
			var name = element.GetProperty("name").GetString()!;

			var componentsJson = element.GetProperty("components");
			List<Component> components = new(componentsJson.GetArrayLength());
			foreach (var componentJson in componentsJson.EnumerateArray())
			{
				components.Add(ComponentAsset.BuildFromJson(componentJson));
			}
			return scene.CreateEntity(name, components.ToArray());
		}

		public static void SaveToJson(Entity entity, Utf8JsonWriter doc)
		{
			doc.WriteStartObject();
			doc.WriteString("name", entity.Name);
			doc.WritePropertyName("components");
			doc.WriteStartArray();
			foreach (var component in entity.Components)
			{
				ComponentAsset.SaveToJson(component, doc);
			}
			doc.WriteEndArray();
			doc.WriteEndObject();
		}
	}

	public static class SceneAsset
	{
		public static Scene BuildFromJson(JsonElement element)
		{
			var nameJson = element.GetProperty("name");
			var entitiesJson = element.GetProperty("entities");
			Scene scene = new(nameJson.GetString()!);
			foreach (var entityJson in entitiesJson.EnumerateArray())
				EntityAsset.BuildFromJson(entityJson, scene);
			return scene;
		}

		public static Scene BuildFromFile(string path)
		{
			var doc = JsonDocument.Parse(File.ReadAllText(path));
			return BuildFromJson(doc.RootElement);
		}

		public static void SaveToJson(Scene scene, Utf8JsonWriter doc)
		{
			doc.WriteStartObject();
			doc.WriteString("name", scene.Name);
			doc.WritePropertyName("entities");
			doc.WriteStartArray();
			foreach (var entity in scene.Entities)
			{
				EntityAsset.SaveToJson(entity, doc);
			}
			doc.WriteEndArray();
			doc.WriteEndObject();
		}

		public static void SaveToFile(Scene scene, string path, bool pretty = false)
		{
			using var fout = new FileStream(path, FileMode.Create);
			using var doc = new Utf8JsonWriter(fout, new JsonWriterOptions { Indented = pretty });
			SaveToJson(scene, doc);
		}
	}
}
