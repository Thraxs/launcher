﻿using System;
using System.Reflection;
using _11thLauncher.Net;

namespace _11thLauncher
{
    class Program
    {
        [STAThread]
        public static void Main(string[] appArgs)
        {
            //Load libraries with reflection
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                var resourceName = "_" + Assembly.GetExecutingAssembly().GetName().Name + ".lib." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            };

            //Check startup parameters
            if (appArgs.Length != 0)
            {
                switch (appArgs[0])
                {
                    case "-updated":
                        Updater.Updated = true;
                        break;

                    case "-updateFailed":
                        Updater.UpdateFailed = true;
                        break;

                    default:
                        break;
                }
            }

            //Check startup parameters
            App.Main();
        }
    }
}
