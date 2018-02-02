using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Zequel
{
    public class ZequelRerunner
    {
        private static int ERR_INSUFFICIENT_ARGS = -1;

        private static string ARG_MAX_COMMAND = "command";
        private static string OPT_MAX_RERUNS = "-$ <max>";

        public static void Main(string[] args)
        {
            CommandLineApplication cmdApp = new CommandLineApplication(throwOnUnexpectedArg: false);
            cmdApp.Name = "zequel";
            cmdApp.FullName = "Zequel Rerunner Tool";
            cmdApp.HelpOption("--h");

            var custom = cmdApp.Command("custom", app => 
            {
                app.Description = "Reruns a custom command that is passed as an argument";
                app.Argument(ARG_MAX_COMMAND, "The command that will be rerun (surround by \"\")");
                //app.Argument(ARG_MAX_RERUNS, "The maximum amount of times which the command will be rerun. Leave blank or -1 for infinit reruns.");

                var maxOpt = app.Option(
                    OPT_MAX_RERUNS, 
                    "The maximum amount of times which the command will be rerun. Leave blank or -1 for infinit reruns.", 
                    CommandOptionType.SingleValue
                );

                maxOpt.LongName = "Maximum Reruns";
                maxOpt.ShortName = "Max Reruns";

                app.HelpOption("--h");
                app.OnExecute(() => Rerun(app.Arguments, maxOpt));
            });

            cmdApp.Execute(args);
        }

        public static int Rerun(IList<CommandArgument> args, CommandOption maxOpt)
        {
            if (args == null || !args.Any())
                return ERR_INSUFFICIENT_ARGS;

            int runs = 0;
            int reruns = Int32.MaxValue;

            string cmd = args.FirstOrDefault(a => a.Name == ARG_MAX_COMMAND)?.Value;
            cmd = $"/C {cmd}";

            if (maxOpt != null && maxOpt.Values.Any())
            {
                int max = -1;
                Int32.TryParse(maxOpt.Values.First(), out max);

                reruns = max > 0 ? max : reruns;
            }

            do
            {
                Console.WriteLine("\n==== Running Zequel Command ====\n\n");

                ProcessStartInfo procInfo = new ProcessStartInfo("CMD.exe", cmd);
                var proc = Process.Start(procInfo);

                // waiting while process is running
                proc.WaitForExit();

                Console.WriteLine($"\nProcess exited with code: {proc.ExitCode}");
            }
            while (reruns > ++runs);

            return 0;
        }
    }
}
