namespace Jossellware.Shared.Networking.Ip
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;

	public readonly struct MacAddress : IEquatable<MacAddress>, IFormattable
	{
		public static MacAddress Zero = new MacAddress(new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

		public static MacAddress Create(ReadOnlySpan<byte> hardwareAddressBytes)
		{
			return new MacAddress(hardwareAddressBytes);
		}

		public static MacAddress Parse(string hardwareAddress)
		{
			try
			{
				return new MacAddress(hardwareAddress);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException)
			{
				throw new FormatException("Incorrectly formatted MAC address", ex);
			}
		}

		public static MacAddress Parse(string hardwareAddress, char delimiter)
		{
			try
			{
				return new MacAddress(hardwareAddress, delimiter);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException)
			{
				throw new FormatException("Incorrectly formatted hardware MAC address", ex);
			}
		}

		public static bool TryParse(string hardwareAddress, out MacAddress result)
		{
			return TryParse(hardwareAddress, DefaultByteDelimiter, out result);
		}

		public static bool TryParse(string hardwareAddress, char delimiter, out MacAddress result)
		{
			result = default(MacAddress);
			if (MacAddress.IsValid(hardwareAddress, delimiter))
			{
				try
				{
					result = Parse(hardwareAddress, delimiter);
					return true;
				}
				catch (ArgumentException)
				{
				}
				catch (FormatException)
				{
				}
			}

			return false;
		}

		public static bool IsValid(string hardwareAddress, char delimiter)
		{
			if (delimiter != DefaultByteDelimiter)
			{
				hardwareAddress = hardwareAddress.Replace(delimiter, DefaultByteDelimiter);
			}

			return IsValid(hardwareAddress);
		}

		public static bool IsValid(string hardwareAddress)
		{
			return !string.IsNullOrWhiteSpace(hardwareAddress) &&
				Regex.IsMatch(hardwareAddress, MacAddressRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}

		public const char DefaultByteDelimiter = ':';
		private const string MacAddressRegex = @"(?:[a-fA-F0-9]{2}\:){5}[a-fA-F0-9]{2}";

		private readonly byte[] addressBytes;

		public ReadOnlySpan<byte> Bytes
		{
			get => this.addressBytes;
		}

		private MacAddress(string hardwareAddress)
		{
			if (!IsValid(hardwareAddress))
			{
				throw new ArgumentException("Not a valid MAC address", nameof(hardwareAddress));
			}

			this.addressBytes = hardwareAddress
				.Split(DefaultByteDelimiter, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => byte.Parse(x.Trim(), NumberStyles.HexNumber))
				.ToArray();
		}

		private MacAddress(ReadOnlySpan<byte> hardwareAddress)
		{
			if (hardwareAddress.Length != 6)
			{
				throw new ArgumentException("Not a valid MAC address", nameof(hardwareAddress));
			}

			this.addressBytes = hardwareAddress.ToArray();
		}

		private MacAddress(string hardwareAddress, char delimiter)
			: this(hardwareAddress.Replace(delimiter, DefaultByteDelimiter))
		{
		}

		public override string ToString()
		{
			return string.Join(DefaultByteDelimiter, this.addressBytes.Select(x => x.ToString("X2")));
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (format == "D" || format == ":")
			{
				return this.ToString();
			}
			else if (format == "W" || format == "-")
			{
				return this.ToDelimitedString('-');
			}
			else if (format == "_")
			{
				return this.ToDelimitedString('_');
			}
			else if (format == "X")
			{
				return string.Concat(this.Bytes.ToArray().Select(x => x.ToString("X2")));
			}

			throw new FormatException($"Invalid format: {format}");
		}

		public string ToDelimitedString(char delimiter)
		{
			return string.Join(delimiter, this.addressBytes.Select(x => x.ToString("X2")));
		}

		public bool Equals(MacAddress other)
		{
			return this.addressBytes.SequenceEqual(other.addressBytes);
		}

		public override bool Equals(object? obj)
		{
			return obj != null &&
				obj is MacAddress hardwareAddress &&
				hardwareAddress.Equals(this);
		}

		public override int GetHashCode()
		{
            return HashCode.Combine(2337 * 23, this.addressBytes);
		}

		public static bool operator ==(MacAddress left, MacAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MacAddress left, MacAddress right)
		{
			return !(left == right);
		}
	}
}
