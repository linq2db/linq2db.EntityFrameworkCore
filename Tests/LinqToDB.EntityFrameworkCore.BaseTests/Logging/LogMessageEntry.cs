﻿using System;

namespace LinqToDB.EntityFrameworkCore.BaseTests.Logging
{
	internal readonly record struct LogMessageEntry(
		string Message,
		string? TimeStamp = null,
		string? LevelString = null,
		ConsoleColor? LevelBackground = null,
		ConsoleColor? LevelForeground = null,
		ConsoleColor? MessageColor = null,
		bool LogAsError = false);
}
