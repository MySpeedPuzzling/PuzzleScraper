using Arctic.Puzzlers.Objects.PuzzleObjects;
using System.ComponentModel.DataAnnotations;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arctic.Puzzlers.Objects.Misc
{
    public class BrandNameJsonConvert : JsonConverter<BrandName>
    {
        public override BrandName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString().GetEnumFromString<BrandName>();
        }

        public override void Write(Utf8JsonWriter writer, BrandName value, JsonSerializerOptions options)
        {
            var attribute = value.GetAttributeOfType<DisplayAttribute>();
            if(attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                writer.WriteStringValue(attribute.Name);
            }
            else
            {
                writer.WriteStringValue(Enum.GetName(value));
            }
        }
    }
}
