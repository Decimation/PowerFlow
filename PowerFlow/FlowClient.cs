using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using static PowerFlow.Ext;

[assembly: InternalsVisibleTo("PowerFlow.Cli")]

namespace PowerFlow;

public class FlowClient:IDisposable, IAsyncDisposable
{
	public SQLiteConnection Client { get; }

	public FlowClient() : this(FindDatabase()) { }

	public FlowClient(string db)
	{
		Client = new SQLiteConnection($"Data Source={db}");
		
		Client.Update += (sender, args) =>
		{
			Console.WriteLine($"{sender} {args}");
		};
	}

	public async Task Open()
	{
		await Client.OpenAsync();

	}

	public async Task<FlowData> Read()
	{
		using var c = Client.CreateCommand();

		c.CommandText = @"
SELECT * FROM NotificationData ORDER BY Ticks DESC LIMIT 1
";
		using var o = await c.ExecuteReaderAsync();
		if (await o.ReadAsync()) {
			var id           = o.GetString(0);
			var notiCategory = o.GetInt32(1);
			var name         = o.GetString(3);
			var group        = o.GetString(4);
			var tag          = o.GetString(5);
			var isRead       = o.GetInt32(6).ToBool();
			var iconPath     = o.GetString(7);
			var o1           = o["Title"];
			var o2           = o["Url"];
			var o3           = o["Content"];
			var fs           = Parse2<long>(o["FileSize"]);

			var x = new FlowData()
			{
				UniqueID       = id,
				NotiCategory   = notiCategory,
				AppDisplayName = name,
				Group          = group,
				Tag            = tag,
				IsRead         = isRead,
				IconPath       = iconPath,
				Title          = Parse<string>(o1),
				Url            = Parse<string>(o2),
				Content        = Parse<string>(o3),
				FileSize       = fs
			};

			return x;

		}

		return null;
	}

	public static string FindDatabase()
	{
		var appdata = Path.Combine(Environment.GetEnvironmentVariable("localappdata"), "Packages");
		var flow    = Directory.EnumerateDirectories(appdata, "*flux*").FirstOrDefault();

		if (flow == null) {
			throw new FileNotFoundException();
		}

		var flowState = Path.Combine(flow, "LocalState");
		var db        = Path.Combine(flowState, "Notifications.db");

		return db;
	}

	public void Dispose()
	{
		Client.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		await Client.DisposeAsync();
	}
}

public static class Ext
{
	public static T Parse<T>(object? o)
	{
		if (o is DBNull or null) {
			return default;
		}

		return (T) o;
	}

	public static T? Parse2<T>(object? o) where T : IParsable<T>
	{
		if (o is DBNull or null) {
			return default;
		}

		return T.Parse(o.ToString(), CultureInfo.InvariantCulture);
	}

	public static bool ToBool<T>(this T? t) where T : INumber<T>
	{
		return T.One == t;
	}
}

public class FlowData
{
	public string UniqueID     { get; internal set; }
	public int    NotiCategory { get; internal set; }

	public string PackageName    { get; internal set; }
	public string AppDisplayName { get; internal set; }

	public string Group { get; internal set; }

	public string Tag { get; internal set; }

	public bool IsRead { get; internal set; }

	public string IconPath { get; internal set; }

	public string Title   { get; internal set; }
	public string Content { get; internal set; }

	public long   Ticks             { get; internal set; }
	public string Url               { get; internal set; }
	public long   FileSize          { get; internal set; }
	public long   CompletedFileSize { get; internal set; }

	public int IsRemovedFile { get; internal set; }

	internal FlowData() { }

	public override string ToString()
	{
		return
			$"{nameof(UniqueID)}: {UniqueID}, {nameof(NotiCategory)}: {NotiCategory}, {nameof(PackageName)}: {PackageName}," +
			$" {nameof(AppDisplayName)}: {AppDisplayName}, {nameof(Group)}: {Group}, {nameof(Tag)}: {Tag}, {nameof(IsRead)}: {IsRead}, " +
			$"{nameof(IconPath)}: {IconPath}, {nameof(Title)}: {Title}, {nameof(Content)}: {Content}, {nameof(Ticks)}: {Ticks}, " +
			$"{nameof(Url)}: {Url}, {nameof(FileSize)}: {FileSize}, {nameof(CompletedFileSize)}: {CompletedFileSize}, " +
			$"{nameof(IsRemovedFile)}: {IsRemovedFile}";
	}
}