using System.Text.Json;

namespace BattleSimulator.Server.Database;

public interface IJsonSerializerWrapper {
    T? DeserializeFile<T>(string filePath);
}

public class JsonSerializerWrapper: IJsonSerializerWrapper {
    public T? DeserializeFile<T>(string filePath) {
        string? content;
        try {
            content = File.ReadAllText(filePath);
        } catch (Exception) {
            return default;
        }
        return JsonSerializer.Deserialize<T>(content);
    }
}