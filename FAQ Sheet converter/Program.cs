using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FAQ_Sheet_converter
{
    internal enum Game
    {
        RE1,
        RE2,
        RE3,
        ALL
    }

    internal enum Platform
    {
        Dolphin,
        PC,
        ALL
    }

    internal enum Target
    {
        Discord,
        Website,
        ALL
    }

    internal class FaqEntry
    {
        public string Question;
        public string Answer;
        public Game Game;
        public Platform Platform;
        public Target target;
    }

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("No or too many files has been dropped on the application...");
                    Console.ReadKey(true);
                    return;
                }

                var faqEntries = new List<FaqEntry>();
                if (!ProcessFaqSheet(args[0], faqEntries))
                {
                    Console.WriteLine($"{args[0]} is not a FAQ tsv file...");
                    Console.ReadKey(true);
                    return;
                }

                Console.WriteLine("[W]ebsite or [D]iscord?");

                var x = Console.ReadKey(true);
                switch (x.KeyChar)
                {
                    case 'W':
                    case 'w':
                        CreateWebsiteFaq(faqEntries);
                        break;


                    case 'D':
                    case 'd':
                        CreateDiscordFaq(faqEntries);
                        break;

                    default:
                        continue;
                }

                Console.WriteLine("Done!");
                Console.ReadKey(true);
                break;
            }
        }

        private static void CreateWebsiteFaq(List<FaqEntry> faqEntries)
        {
            var pathRe1 = Path.Combine(Environment.CurrentDirectory, "Re1.txt");
            var pathRe2 = Path.Combine(Environment.CurrentDirectory, "Re2.txt");
            var pathRe3 = Path.Combine(Environment.CurrentDirectory, "Re3.txt");

            // using var streamWriterRe1 = File.CreateText(pathRe1);
            using var streamWriterRe2 = File.CreateText(pathRe2);
            using var streamWriterRe3 = File.CreateText(pathRe3);

            var stringBuilder = new StringBuilder();
            foreach (var faqEntry in faqEntries)
            {
                stringBuilder.Clear();

                stringBuilder.AppendLine($">**{faqEntry.Question}**<br>");
                stringBuilder.AppendLine(faqEntry.Answer);
                stringBuilder.AppendLine("{: .notice }");
                stringBuilder.AppendLine();

                switch (faqEntry.Game)
                {
                    case Game.RE1:
                        // streamWriterRe1.WriteLine(stringBuilder);
                        break;

                    case Game.RE2:
                        streamWriterRe2.WriteLine(stringBuilder);
                        break;

                    case Game.RE3:
                        streamWriterRe3.WriteLine(stringBuilder);
                        break;

                    case Game.ALL:
                        // streamWriterRe1.WriteLine(stringBuilder);
                        streamWriterRe2.WriteLine(stringBuilder);
                        streamWriterRe3.WriteLine(stringBuilder);
                        break;
                }
            }
        }

        private static void CreateDiscordFaq(List<FaqEntry> faqEntries)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Discord.txt");
            using var streamWriter = File.CreateText(path);
            foreach (var faqEntry in faqEntries)
            {
                streamWriter.WriteLine($"**Q: {faqEntry.Question}**");
                streamWriter.WriteLine($"A: {faqEntry.Answer}");
                streamWriter.WriteLine("‎");
            }
        }

        private static bool ProcessFaqSheet(string path, List<FaqEntry> faqEntries)
        {
            if (faqEntries == null)
                return false;

            if (!File.Exists(path))
                return false;

            var lines = File.ReadAllLines(path);

            if (lines.Length <= 0)
                return false;

            foreach (var line in lines)
            {
                var data = line.Split('\t');

                if (data.Length != 5)
                    return false;

                faqEntries.Add(new FaqEntry
                {
                    Question = data[0],
                    Answer = data[1],
                    Platform = TextToPlatform(data[2]),
                    Game = TextToGame(data[3]),
                    target = TextToTarget(data[4])
                });
            }

            return true;
        }

        private static Platform TextToPlatform(string data) =>
            Enum.TryParse(data, true, out Platform x) ? x : Platform.ALL;

        private static Game TextToGame(string data) =>
            Enum.TryParse(data, true, out Game x) ? x : Game.ALL;

        private static Target TextToTarget(string data) =>
            Enum.TryParse(data, true, out Target x) ? x : Target.ALL;
    }
}