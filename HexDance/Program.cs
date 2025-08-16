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

        public static readonly Option<TimeSpan> EffectDisplayTime = new(
            name: "--effectDisplayTime")
        {
            DefaultValueFactory = _ => Settings.Default.EffectDisplayTime,
            CustomParser = ParseTimeSpanOption,
        };

        public static readonly Option<float> EffectDistance = new(
            name: "--effectDistance")
        {
            DefaultValueFactory = _ => Settings.Default.EffectDistance,
        };

        public static readonly Option<Color> EffectFillColor = new(
            name: "--effectFillColor")
        {
            DefaultValueFactory = _ => Settings.Default.EffectFillColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<float> EffectHexSize = new(
            name: "--effectHexSize")
        {
            DefaultValueFactory = _ => Settings.Default.EffectHexSize,
        };

        public static readonly Option<Color> EffectLineColor = new(
            name: "--effectLineColor")
        {
            DefaultValueFactory = _ => Settings.Default.EffectLineColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<Color> GridBrightColor = new(
            name: "--gridBrightColor")
        {
            DefaultValueFactory = _ => Settings.Default.GridBrightColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<Color> GridDarkColor = new(
            name: "--gridDarkColor")
        {
            DefaultValueFactory = _ => Settings.Default.GridDarkColor,
            CustomParser = ParseColorOption,
        };

        public static readonly Option<TimeSpan> GridDisplayTime = new(
            name: "--gridDisplayTime")
        {
            DefaultValueFactory = _ => Settings.Default.GridDisplayTime,
            CustomParser = ParseTimeSpanOption,
        };

        public static readonly Option<float> GridHexSize = new(
            name: "--gridHexSize")
        {
            DefaultValueFactory = _ => Settings.Default.GridHexSize,
        };

        public static readonly Option<int> GridQueueLength = new(
            name: "--gridQueueLength")
        {
            DefaultValueFactory = _ => Settings.Default.GridQueueLength,
        };

        public static readonly Option<int> GridSegmentCount = new(
            name: "--gridSegmentCount")
        {
            DefaultValueFactory = _ => Settings.Default.GridSegmentCount,
        };

        public static readonly Option<double> Opacity = new(
            name: "--opacity")
        {
            DefaultValueFactory = _ => Settings.Default.Opacity,
        };

        public static readonly Option<TimeSpan> UpdateInterval = new(
            name: "--updateInterval")
        {
            DefaultValueFactory = _ => Settings.Default.UpdateInterval,
            CustomParser = ParseTimeSpanOption,
        };

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            NativeMethods.AttachConsole(NativeMethods.ATTACH_PARENT_PROCESS);

            var rootCommand = new RootCommand();
            rootCommand.Options.Add(ChromaKeyColor);
            rootCommand.Options.Add(DoubleBuffered);
            rootCommand.Options.Add(EffectDisplayTime);
            rootCommand.Options.Add(EffectDistance);
            rootCommand.Options.Add(EffectFillColor);
            rootCommand.Options.Add(EffectHexSize);
            rootCommand.Options.Add(EffectLineColor);
            rootCommand.Options.Add(GridBrightColor);
            rootCommand.Options.Add(GridDarkColor);
            rootCommand.Options.Add(GridDisplayTime);
            rootCommand.Options.Add(GridHexSize);
            rootCommand.Options.Add(GridQueueLength);
            rootCommand.Options.Add(GridSegmentCount);
            rootCommand.Options.Add(Opacity);
            rootCommand.Options.Add(UpdateInterval);

            rootCommand.SetAction(static parseResult =>
            {
                var settings = Settings.Default;
                settings.ChromaKeyColor = parseResult.GetValue(ChromaKeyColor);
                settings.DoubleBuffered = parseResult.GetValue(DoubleBuffered);
                settings.EffectDisplayTime = parseResult.GetValue(EffectDisplayTime);
                settings.EffectDistance = parseResult.GetValue(EffectDistance);
                settings.EffectFillColor = parseResult.GetValue(EffectFillColor);
                settings.EffectHexSize = parseResult.GetValue(EffectHexSize);
                settings.EffectLineColor = parseResult.GetValue(EffectLineColor);
                settings.GridBrightColor = parseResult.GetValue(GridBrightColor);
                settings.GridDarkColor = parseResult.GetValue(GridDarkColor);
                settings.GridDisplayTime = parseResult.GetValue(GridDisplayTime);
                settings.GridHexSize = parseResult.GetValue(GridHexSize);
                settings.GridQueueLength = parseResult.GetValue(GridQueueLength);
                settings.GridSegmentCount = parseResult.GetValue(GridSegmentCount);
                settings.Opacity = parseResult.GetValue(Opacity);
                settings.UpdateInterval = parseResult.GetValue(UpdateInterval);

                ApplicationConfiguration.Initialize();
                Application.Run(new HexDisplay(settings));
            });

            return new CommandLineConfiguration(rootCommand).Invoke(args);
        }

        private static TimeSpan ParseTimeSpanOption(ArgumentResult result) => TimeSpanParser.Parse(string.Join(" ", result.Tokens.Select(t => t.Value)));

        private static Color ParseColorOption(ArgumentResult result) => ColorTranslator.FromHtml(result.Tokens.Single().Value);
    }
}
