using H.NotifyIcon.EfficiencyMode;
using lib;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;

namespace service;

[SupportedOSPlatform("Windows")]
public class Program {
	internal static JsonSerializerOptions CompressedSerializerOptions = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};
	internal static JsonSerializerOptions SerializerOptions = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true,
	};

	internal static Settings Settings = new();

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
		Debug.WriteLine("App crashed");
		SentrySdk.CaptureException((Exception)e.ExceptionObject);
	}

	public static void Main(string[] args) {
		SentrySdk.Init(settings => {
			settings.Dsn = "https://d4ad9ffb8f4333d328052ee6551db833@o4506670275428352.ingest.us.sentry.io/4507197428596736";
			settings.AutoSessionTracking = true;
			settings.IsGlobalModeEnabled = true;
			settings.EnableTracing = true;
		});

		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

		try {
			EfficiencyModeUtilities.SetEfficiencyMode(true);
		}
		catch (Exception e) {
			SentrySdk.CaptureException(e);
		}

		// Set firewall rules
		var ruleExists = Commands.ExecuteCommand(@"netsh advfirewall firewall show rule name='CoresService'");

		if (ruleExists.Contains("No rules match the specified criteria.")) {
			var exe = Path.Join(AppContext.BaseDirectory, "CoresService.exe");
			var res = Commands.ExecuteCommand($"netsh advfirewall firewall add rule name='CoresService' dir=in action=allow program='{exe}' enable=yes profile=private,public");
		}

		HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
		builder.Services.AddWindowsService(options => {
			options.ServiceName = "Cores Service";
		});

		LoggerProviderOptions.RegisterProviderOptions<
			EventLogSettings, EventLogLoggerProvider>(builder.Services);

		builder.Services.AddHostedService<WindowsBackgroundService>();

		IHost host = builder.Build();
		host.Run();
	}
}
