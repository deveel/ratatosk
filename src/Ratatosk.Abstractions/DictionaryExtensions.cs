//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ratatosk {
	/// <summary>
	/// An helper class that provides extensions to
	/// the <see cref="IDictionary{TKey, TValue}"/> contract.
	/// </summary>
	public static class DictionaryExtensions {
		/// <summary>
		/// Tries to get the value associated to the given key
		/// from the dictionary, converting it to the given type.
		/// </summary>
		/// <typeparam name="TValue">
		/// The type of the value to get from the dictionary.
		/// </typeparam>
		/// <param name="dictionary">
		/// The instance of the dictionary to get the value from.
		/// </param>
		/// <param name="key">
		/// The key of the value to get from the dictionary.
		/// </param>
		/// <param name="value">
		/// The value that is returned from the dictionary.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the value was found in the dictionary,
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool TryGetValue<TValue>(this IDictionary<string, object> dictionary, string key, [MaybeNullWhen(false)] out TValue? value) {
			if (!dictionary.TryGetValue(key, out var obj)) {
				value = default;
				return false;
			}
			if (obj is TValue tValue) {
				value = tValue;
				return true;
			}

			var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
			if (targetType.IsEnum && obj is string s) {
				if (Enum.TryParse(targetType, s, true, out var enumValue)) {
					value = (TValue)enumValue!;
					return true;
				}

				value = default;
				return false;
			} else if (obj is IConvertible convertible) {
				try {
					value = (TValue)Convert.ChangeType(convertible, targetType);
					return true;
				} catch (FormatException) {
					value = default;
					return false;
				} catch (InvalidCastException) {
					value = default;
					return false;
				} catch (OverflowException) {
					value = default;
					return false;
				}
			} else if (obj is JsonElement jsonElement) {
				try {
					value = jsonElement.Deserialize<TValue>();
					return true;
				} catch (JsonException) {
					value = default;
					return false;
				} catch (NotSupportedException) {
					value = default;
					return false;
				}
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Merges the given dictionary with the current one,
		/// returning a new dictionary that contains the result
		/// of the merge.
		/// </summary>
		/// <param name="dictionary">
		/// The instance of the dictionary to merge with.
		/// </param>
		/// <param name="other">
		/// The other dictionary to merge with the current one.
		/// </param>
		/// <returns>
		/// Returns a new dictionary that contains the result
		/// of the merge of the two dictionaries.
		/// </returns>
		public static IDictionary<string, TValue> Merge<TValue>(this IDictionary<string, TValue>? dictionary, IDictionary<string, TValue>? other) {
			if (dictionary == null && other == null)
				return new Dictionary<string, TValue>();
			if (dictionary == null && other != null)
				return new Dictionary<string, TValue>(other);
			if (dictionary != null && other == null)
				return new Dictionary<string, TValue>(dictionary);

			var result = new Dictionary<string, TValue>(dictionary!);
			foreach (var (key, value) in other!) {
				if (result.ContainsKey(key)) {
					if (value == null) {
						result.Remove(key);
					} else {
						result[key] = value;
					}
				} else {
					result.Add(key, value);
				}
			}

			return result;
		}
	}
}
