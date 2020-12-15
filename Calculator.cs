using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCalculator {
	static class Calculator {
		public static IPAddress GetNetwork(IPAddress ip, Netmask netmask) => 
			new IPAddress(ip.Bits.And(netmask.Bits));

		public static IPAddress GetNetwork(IPAddress ip, int netmaskLength) =>
			new IPAddress(ip.Bits.And(new Netmask(netmaskLength).Bits));


		public static IPAddress GetBroadcast(IPAddress ip, Netmask netmask) =>
			new IPAddress(ip.Bits.Or(netmask.Bits.Not()));

		public static IPAddress GetBroadcast(IPAddress ip, int netmaskLength) =>
			new IPAddress(ip.Bits.Or(new Netmask(netmaskLength).Bits.Not()));


		public static int GetHostBits(Netmask netmask) =>
			netmask.Bits.Cast<bool>().Where(bit => bit == false).Count();

		public static int GetHostBits(int netmaskLength) => 32 - netmaskLength;


		public static Netmask GetNetmask(int amt) {
			int reqBits = 1;
			for (int i = 0; Math.Pow(reqBits, i) < amt; i++)
				reqBits++;
			return new Netmask(32 - reqBits);
		}
	}
}
