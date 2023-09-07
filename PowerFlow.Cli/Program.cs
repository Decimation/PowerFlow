using System.Globalization;
using System.Numerics;

namespace PowerFlow.Cli;

public static class Program
{
	
	public async static Task Main(string[] args)
	{
		using var c = new FlowClient();
		Console.WriteLine(c.Client);
		await c.Open();
		while (true) {
			var o = await c.Read();
			Console.WriteLine(o);
			Console.ReadKey();
		}
		await c.Client.CloseAsync();
	}
}