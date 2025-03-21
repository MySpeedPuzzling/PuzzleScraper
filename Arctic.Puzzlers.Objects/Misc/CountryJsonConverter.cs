using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace Arctic.Puzzlers.Objects.Misc
{
    internal class CountryJsonConverter : JsonConverter<Countries>
    {
        public override Countries Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string data = string.Empty;
            if(reader.TokenType == JsonTokenType.Number)
            {
                data = reader.GetInt64().ToString();
            }
            else
            {
                data = reader.GetString();
            }
            if (string.IsNullOrWhiteSpace(data))
            {
                return Countries.UNK;
            }
            return data.GetEnumFromString<Countries>();
        }
                

        public override void Write(
            Utf8JsonWriter writer,
            Countries country,
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            JsonSerializerOptions options) =>
                writer.WriteStringValue(country.ToString());
#pragma warning restore CS8602 // Dereference of a possibly null reference.




    }
}
