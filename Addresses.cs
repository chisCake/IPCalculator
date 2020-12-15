using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCalculator {
	abstract class Address {
		public int[] Octets { get; private set; }
		public BitArray Bits { get; private set; }

		public int this[int index] {
			get => Octets[index];
			private set => Octets[index] = value;
		}

		public Address(string address) {
			Octets = ParseToInt(address);
			Bits = GetBits(this);
		}

		public Address(int[] octets) {
			if (octets.Length != 4 || octets.Any(octet => octet < 0 || octet > 255))
				throw new InvalidAddressFormatException();
			Octets = octets;
			Bits = GetBits(this);
		}

		public Address(BitArray bits) {
			Octets = ParseToInt(bits);
			Bits = bits;
		}

		protected Address(int length) {
			if (length < 0 || length > 32)
				throw new InvalidNetmaskFormatException();

			var bits = new BitArray(32);
			for (int i = 0; i < 32; i++)
				bits[i] = i < length;

			Octets = ParseToInt(bits);
			Bits = bits;
		}

		public static BitArray GetBits(Address address) {
			var bits = new BitArray(32);
			for (int i = 0; i < 4; i++) {
				BitArray b = new BitArray(new byte[] { (byte)address.Octets[i] });
				for (int j = 7, pos = 8 * i; j >= 0; j--, pos++)
					bits[pos] = b[j];
			}
			return bits;
		}

		protected static int[] ParseToInt(string address) {
			var strOctets = address.Split('.');
			if (strOctets.Length != 4)
				throw new InvalidAddressFormatException();
			var octets = new int[4];
			for (int i = 0; i < 4; i++)
				if (!int.TryParse(strOctets[i], out octets[i]) || octets[i] < 0 || octets[i] > 255)
					throw new InvalidAddressFormatException();
			return octets;
		}

		protected static int[] ParseToInt(BitArray bits) {
			if (bits.Length != 32)
				throw new InvalidAddressFormatException("BitArray != 32");
			var octets = new int[4];
			for (int i = 0; i < 4; i++) {
				int octet = 0;
				for (int j = 7, pos = 8 * i; j >= 0; j--, pos++)
					if (bits[pos] == true)
						octet += Math.Pow(2, j);
				octets[i] = octet;
			}
			return octets;
		}

		public string ToString(string format) {
			switch (format) {
				case "b":
					string bits = "";
					for (int i = 0; i < Bits.Length; i++) {
						if (i % 8 == 0 && i != 0)
							bits += ".";
						bits += Bits[i] ? "1" : "0";
					}
					return bits;
				default:
					return $"{Octets[0]}.{Octets[1]}.{Octets[2]}.{Octets[3]}";
			}
		}

		public override string ToString() => $"{Octets[0]}.{Octets[1]}.{Octets[2]}.{Octets[3]}";
	}

	class IPAddress : Address {
		public IPAddress(string address) : base(address) { }
		public IPAddress(BitArray bits) : base(bits) { }
	}

	class Netmask : Address {
		public int Length { get; private set; }

		public Netmask(string address) : base(address) {
			var octets = ParseToInt(address);
			if (!Check(octets))
				throw new InvalidNetmaskFormatException();
			Length = Bits.Cast<bool>().Where(bit => bit == true).Count();
		}

		public Netmask(BitArray bits) : base(bits) {
			var octets = ParseToInt(bits);
			if (!Check(octets))
				throw new InvalidNetmaskFormatException();
			Length = Bits.Cast<bool>().Where(bit => bit == true).Count();
		}

		public Netmask(int length) : base(length) => Length = length;

		public static int GetLength(string address) =>
			new Netmask(address).Length;

		static bool Check(int[] octets) {
			for (int i = 0; i < 4; i++)
				if (!IsCorrect(octets[i]))
					return false;
			return true;

			bool IsCorrect(int num) {
				for (int i = 0; i < 9; i++)
					if (256 - Math.Pow(2, i) == num)
						return true;
				return false;
			}
		}
	}

	static class Math {
		public static int Pow(int a, int b) {
			if (b == 0)
				return 1;
			if (b == 1)
				return a;
			int res = a;
			for (int i = 1; i < b; i++)
				res *= a;
			return res;
		}
	}

	[Serializable]
	public class InvalidAddressFormatException : Exception {
		public InvalidAddressFormatException() { }
		public InvalidAddressFormatException(string message) : base(message) { }
		public InvalidAddressFormatException(string message, Exception inner) : base(message, inner) { }
		protected InvalidAddressFormatException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InvalidNetmaskFormatException : InvalidAddressFormatException {
		public InvalidNetmaskFormatException() { }
		public InvalidNetmaskFormatException(string message) : base(message) { }
		public InvalidNetmaskFormatException(string message, Exception inner) : base(message, inner) { }
		protected InvalidNetmaskFormatException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
