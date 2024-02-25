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
            JsonSerializerOptions options) =>
                reader.GetString().GetValueFromShortName<Countries>();

        public override void Write(
            Utf8JsonWriter writer,
            Countries country,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(country.GetAttributeOfType<DisplayAttribute>().ShortName);




    }
}
