// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace HexDance
{
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using HexDance.Properties;
    using TimeSpanParserUtil;

    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    public static class Program
    {
        public static readonly Option<TimeSpan> UpdateInterval = new(
            name: "--updateInterval")
        {
            DefaultValueFactory = _ => Settings.Default.UpdateInterval,
            CustomParser = ParseTimeSpanOption,
        };

        public static readonly Option<TimeSpan> DisplayTime = new(
            name: "--displayTime")
        {
            DefaultValueFactory = _ => Settings.Default.DisplayTime,
            CustomParser = ParseTimeSpanOption,
        };

        public static readonly Option<int> PathQueueLength = new(
            name: "--pathQueueLength")
        {
            DefaultValueFactory = _ => Settings.Default.PathQueueLength,
        };

        public static readonly Option<int> PathSegmentCount = new(
            name: "--pathSegmentCount")
        {
            DefaultValueFactory = _ => Settings.Default.PathSegmentCount,
        };

        public static readonly Option<float> HexGridSize = new(
            name: "--hexGridSize")
        {
            DefaultValueFactory = _ => Settings.Default.HexGridSize,
        };

        public static readonly Option<Color> BrightColor = new(
            name: "--brightColor")
        {
            DefaultValueFactory = _ => Settings.Default.BrightColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<Color> DarkColor = new(
            name: "--darkColor")
        {
            DefaultValueFactory = _ => Settings.Default.DarkColor,
            CustomParser = ParseColorOption,
        };

        private static TimeSpan ParseTimeSpanOption(ArgumentResult result) => TimeSpanParser.Parse(string.Join(" ", result.Tokens.Select(t => t.Value)));

        private static Color ParseColorOption(ArgumentResult result) => ColorTranslator.FromHtml(result.Tokens.Single().Value);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.Options.Add(UpdateInterval);
            rootCommand.Options.Add(DisplayTime);
            rootCommand.Options.Add(PathQueueLength);
            rootCommand.Options.Add(PathSegmentCount);
            rootCommand.Options.Add(HexGridSize);
            rootCommand.Options.Add(BrightColor);
            rootCommand.Options.Add(DarkColor);

            rootCommand.SetAction(static parseResult =>
            {
                var settings = Settings.Default;
                settings.UpdateInterval = parseResult.GetValue(UpdateInterval);
                settings.DisplayTime = parseResult.GetValue(DisplayTime);
                settings.PathQueueLength = parseResult.GetValue(PathQueueLength);
                settings.PathSegmentCount = parseResult.GetValue(PathSegmentCount);
                settings.HexGridSize = parseResult.GetValue(HexGridSize);
                settings.BrightColor = parseResult.GetValue(BrightColor);
                settings.DarkColor = parseResult.GetValue(DarkColor);

                ApplicationConfiguration.Initialize();
                Application.Run(new HexDisplay(settings));
            });

            return new CommandLineConfiguration(rootCommand).Invoke(args);
        }
    }
}
