//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ratatosk {
	/// <summary>
	/// A JSON converter that encodes and decodes strings as Base64.
	/// </summary>
	class Base64JsonConverter : JsonConverter<string> {
		/// <summary>
		/// Reads a Base64-encoded string from the JSON reader and decodes it.
		/// </summary>
		/// <param name="reader">The JSON reader to read from.</param>
		/// <param name="typeToConvert">The type to convert.</param>
		/// <param name="options">Serializer options.</param>
		/// <returns>The decoded UTF-8 string, or null if the token is not a valid string.</returns>
		public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			string? s;
			if (reader.TokenType != JsonTokenType.String || (s = reader.GetString()) == null)
				return null;

			var buffer = new byte[((s.Length * 3) + 3) / 4 -
				(s.Length > 0 && s[s.Length - 1] == '=' ?
				s.Length > 1 && s[s.Length - 2] == '=' ?
				2 : 1 : 0)];

			if (!Convert.TryFromBase64String(s, buffer, out var bytes))
				return null;

			return Encoding.UTF8.GetString(buffer);
		}

		/// <summary>
		/// Writes a string value as a Base64-encoded JSON string.
		/// </summary>
		/// <param name="writer">The JSON writer to write to.</param>
		/// <param name="value">The string value to encode as Base64.</param>
		/// <param name="options">Serializer options.</param>
		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) {
			var bytes = Encoding.UTF8.GetBytes(value);
			var base64 = Convert.ToBase64String(bytes);

			writer.WriteStringValue(base64);
		}
	}
}
