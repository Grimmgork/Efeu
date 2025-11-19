using LinqToDB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class JsonValueConverter<T> : ValueConverter<T, string>
    {
        public JsonValueConverter(JsonSerializerOptions options) : base(
            value => JsonSerializer.Serialize<T>(value, options),
            json => JsonSerializer.Deserialize<T>(json, options), false)
        {

        }
    }
}
