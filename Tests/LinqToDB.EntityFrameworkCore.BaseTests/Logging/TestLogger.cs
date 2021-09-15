using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Logging
{
	internal class TestLogger : ILogger
	{
		private static readonly string _loglevelPadding = ": ";
		private static readonly string _messagePadding;
		private static readonly string _newLineWithMessagePadding;

		// ConsoleColor does not have a value to specify the 'Default' color
#pragma warning disable 649
		private readonly ConsoleColor? DefaultConsoleColor;
#pragma warning restore 649

		private readonly string _name;

		[ThreadStatic]
		private static StringBuilder? _logBuilder;

		static TestLogger()
		{
			var logLevelString = GetLogLevelString(LogLevel.Information);
			_messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
			_newLineWithMessagePadding = Environment.NewLine + _messagePadding;
		}

		internal TestLogger(string name)
		{
			_name = name ?? throw new ArgumentNullException(nameof(name));
		}

		internal IExternalScopeProvider? ScopeProvider { get; set; }

		internal ConsoleLoggerOptions? Options { get; set; }

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			var message = formatter(state, exception);

			if (!string.IsNullOrEmpty(message) || exception != null)
			{
				WriteMessage(logLevel, _name, eventId.Id, message, exception);
			}
		}

		public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
		{
			var format = Options!.FormatterName;
			Debug.Assert(format is ConsoleFormatterNames.Simple or ConsoleFormatterNames.Systemd);

			var logBuilder = _logBuilder;
			_logBuilder = null;

			if (logBuilder == null)
			{
				logBuilder = new StringBuilder();
			}

			LogMessageEntry entry;
			if (format == ConsoleFormatterNames.Simple)
			{
				entry = CreateDefaultLogMessage(logBuilder, logLevel, logName, eventId, message, exception);
			}
			else if (format == ConsoleFormatterNames.Systemd)
			{
				entry = CreateSystemdLogMessage(logBuilder, logLevel, logName, eventId, message, exception);
			}
			else
			{
				entry = default;
			}
			EnqueueMessage(entry);

			logBuilder.Clear();
			if (logBuilder.Capacity > 1024)
			{
				logBuilder.Capacity = 1024;
			}
			_logBuilder = logBuilder;
		}

		private void EnqueueMessage(LogMessageEntry entry)
		{
			WriteMessage(entry);
		}

		internal virtual void WriteMessage(LogMessageEntry message)
		{
			if (message.TimeStamp != null)
			{
				Console.Write(message.TimeStamp, message.MessageColor, message.MessageColor);
			}

			if (message.LevelString != null)
			{
				Console.Write(message.LevelString);
			}

			Console.WriteLine(message.Message);
		}

		private LogMessageEntry CreateDefaultLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception exception)
		{
			// Example:
			// INFO: ConsoleApp.Program[10]
			//       Request received

			var logLevelColors = GetLogLevelConsoleColors(logLevel);
			var logLevelString = GetLogLevelString(logLevel);
			// category and event id
			logBuilder.Append(_loglevelPadding)
				.Append(logName)
				.Append('[')
				.Append(eventId)
				.AppendLine("]");

			// scope information
			GetScopeInformation(logBuilder, multiLine: true);

			if (!string.IsNullOrEmpty(message))
			{
				// message
				logBuilder.Append(_messagePadding);

				var len = logBuilder.Length;
				logBuilder.AppendLine(message);
				logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
			}

			// Example:
			// System.InvalidOperationException
			//    at Namespace.Class.Function() in File:line X
			if (exception != null)
			{
				// exception message
				logBuilder.AppendLine(exception.ToString());
			}

#pragma warning disable CS0618 // Type or member is obsolete
			var timestampFormat = Options!.TimestampFormat;
#pragma warning restore CS0618 // Type or member is obsolete

			return new LogMessageEntry(
				message: logBuilder.ToString(),
				timeStamp: timestampFormat != null ? DateTime.Now.ToString(timestampFormat) : null,
				levelString: logLevelString,
				levelBackground: logLevelColors.Background,
				levelForeground: logLevelColors.Foreground,
				messageColor: DefaultConsoleColor,
				logAsError: logLevel >= Options!.LogToStandardErrorThreshold
			);
		}

		private LogMessageEntry CreateSystemdLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception exception)
		{
			// systemd reads messages from standard out line-by-line in a '<pri>message' format.
			// newline characters are treated as message delimiters, so we must replace them.
			// Messages longer than the journal LineMax setting (default: 48KB) are cropped.
			// Example:
			// <6>ConsoleApp.Program[10] Request received

			// loglevel
			var logLevelString = GetSyslogSeverityString(logLevel);
			logBuilder.Append(logLevelString);

			// timestamp
#pragma warning disable CS0618 // Type or member is obsolete
			var timestampFormat = Options!.TimestampFormat;
#pragma warning restore CS0618 // Type or member is obsolete
			if (timestampFormat != null)
			{
				logBuilder.Append(DateTime.Now.ToString(timestampFormat));
			}

			// category and event id
			logBuilder.Append(logName)
				.Append('[')
				.Append(eventId)
				.Append(']');

			// scope information
			GetScopeInformation(logBuilder, multiLine: false);

			// message
			if (!string.IsNullOrEmpty(message))
			{
				logBuilder.Append(' ');
				// message
				AppendAndReplaceNewLine(logBuilder, message);
			}

			// exception
			// System.InvalidOperationException at Namespace.Class.Function() in File:line X
			if (exception != null)
			{
				logBuilder.Append(' ');
				AppendAndReplaceNewLine(logBuilder, exception.ToString());
			}

			// newline delimiter
			logBuilder.Append(Environment.NewLine);

			return new LogMessageEntry(
				message: logBuilder.ToString(),
				logAsError: logLevel >= Options.LogToStandardErrorThreshold
			);

			static void AppendAndReplaceNewLine(StringBuilder sb, string message)
			{
				var len = sb.Length;
				sb.Append(message);
				sb.Replace(Environment.NewLine, " ", len, message.Length);
			}
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel != LogLevel.None;
		}

		public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

		private static string GetLogLevelString(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Trace:
					return "trce";
				case LogLevel.Debug:
					return "dbug";
				case LogLevel.Information:
					return "info";
				case LogLevel.Warning:
					return "warn";
				case LogLevel.Error:
					return "fail";
				case LogLevel.Critical:
					return "crit";
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel));
			}
		}

		private static string GetSyslogSeverityString(LogLevel logLevel)
		{
			// 'Syslog Message Severities' from https://tools.ietf.org/html/rfc5424.
			switch (logLevel)
			{
				case LogLevel.Trace:
				case LogLevel.Debug:
					return "<7>"; // debug-level messages
				case LogLevel.Information:
					return "<6>"; // informational messages
				case LogLevel.Warning:
					return "<4>"; // warning conditions
				case LogLevel.Error:
					return "<3>"; // error conditions
				case LogLevel.Critical:
					return "<2>"; // critical conditions
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel));
			}
		}

		private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (Options!.DisableColors)
#pragma warning restore CS0618 // Type or member is obsolete
			{
				return new ConsoleColors(null, null);
			}

			// We must explicitly set the background color if we are setting the foreground color,
			// since just setting one can look bad on the users console.
			switch (logLevel)
			{
				case LogLevel.Critical:
					return new ConsoleColors(ConsoleColor.White, ConsoleColor.Red);
				case LogLevel.Error:
					return new ConsoleColors(ConsoleColor.Black, ConsoleColor.Red);
				case LogLevel.Warning:
					return new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black);
				case LogLevel.Information:
					return new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black);
				case LogLevel.Debug:
					return new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black);
				case LogLevel.Trace:
					return new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black);
				default:
					return new ConsoleColors(DefaultConsoleColor, DefaultConsoleColor);
			}
		}

		private void GetScopeInformation(StringBuilder stringBuilder, bool multiLine)
		{
			var scopeProvider = ScopeProvider;
#pragma warning disable CS0618 // Type or member is obsolete
			if (Options!.IncludeScopes && scopeProvider != null)
#pragma warning restore CS0618 // Type or member is obsolete
			{
				var initialLength = stringBuilder.Length;

				scopeProvider.ForEachScope((scope, state) =>
				{
					var (builder, paddAt) = state;
					var padd = paddAt == builder.Length;
					if (padd)
					{
						builder.Append(_messagePadding);
						builder.Append("=> ");
					}
					else
					{
						builder.Append(" => ");
					}
					builder.Append(scope);
				}, (stringBuilder, multiLine ? initialLength : -1));

				if (stringBuilder.Length > initialLength && multiLine)
				{
					stringBuilder.AppendLine();
				}
			}
		}

		private readonly struct ConsoleColors
		{
			public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
			{
				Foreground = foreground;
				Background = background;
			}

			public ConsoleColor? Foreground { get; }

			public ConsoleColor? Background { get; }
		}
	}

}
