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

        public static readonly Option<Color> ChromaKeyColor = new(
            name: "--chromaKey")
        {
            DefaultValueFactory = _ => Settings.Default.ChromaKeyColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<bool> DoubleBuffered = new(
            name: "--doubleBuffered")
        {
            DefaultValueFactory = _ => Settings.Default.DoubleBuffered,
        };

        public static readonly Option<double> Opacity = new(
            name: "--opacity")
        {
            DefaultValueFactory = _ => Settings.Default.Opacity,
        };

        public static readonly Option<float> EffectHexSize = new(
            name: "--effectHexSize")
        {
            DefaultValueFactory = _ => Settings.Default.EffectHexSize,
        };

        public static readonly Option<float> EffectDistance = new(
            name: "--effectDistance")
        {
            DefaultValueFactory = _ => Settings.Default.EffectDistance,
        };

        public static readonly Option<Color> EffectLineColor = new(
            name: "--lineColor")
        {
            DefaultValueFactory = _ => Settings.Default.EffectLineColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<Color> EffectFillColor = new(
            name: "--fillColor")
        {
            DefaultValueFactory = _ => Settings.Default.EffectFillColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<TimeSpan> EffectDisplayTime = new(
            name: "--effectDisplayTime")
        {
            DefaultValueFactory = _ => Settings.Default.EffectDisplayTime,
            CustomParser = ParseTimeSpanOption,
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
            rootCommand.Options.Add(ChromaKeyColor);
            rootCommand.Options.Add(DoubleBuffered);
            rootCommand.Options.Add(Opacity);
            rootCommand.Options.Add(EffectHexSize);
            rootCommand.Options.Add(EffectDistance);
            rootCommand.Options.Add(EffectFillColor);
            rootCommand.Options.Add(EffectLineColor);
            rootCommand.Options.Add(EffectDisplayTime);

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
                settings.ChromaKeyColor = parseResult.GetValue(ChromaKeyColor);
                settings.DoubleBuffered = parseResult.GetValue(DoubleBuffered);
                settings.Opacity = parseResult.GetValue(Opacity);
                settings.EffectHexSize = parseResult.GetValue(EffectHexSize);
                settings.EffectDistance = parseResult.GetValue(EffectDistance);
                settings.EffectFillColor = parseResult.GetValue(EffectFillColor);
                settings.EffectLineColor = parseResult.GetValue(EffectLineColor);
                settings.EffectDisplayTime = parseResult.GetValue(EffectDisplayTime);

                ApplicationConfiguration.Initialize();
                Application.Run(new HexDisplay(settings));
            });

            return new CommandLineConfiguration(rootCommand).Invoke(args);
        }
    }
}
