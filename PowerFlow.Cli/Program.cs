using System.Globalization;
using System.Numerics;

namespace PowerFlow.Cli;

public static class Program
{
	
	public async static Task Main(string[] args)
	{
		var c = new FlowClient();
		Console.WriteLine(c.Client);
		await c.Open();

		var o = await c.Read();
		Console.WriteLine(o);
	}
}