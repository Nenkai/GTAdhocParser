﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GTAdhocTools.UI
{
    public class WidgetDefinitions
    {

        public static Dictionary<string, string> Types = new();

        static WidgetDefinitions()
        {
            Read();
        }

        public static void Read()
        {
            var txt = File.ReadAllLines("UIWidgetDefinitions.txt");
            foreach (var line in txt)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                    continue;

                var spl = line.Split('|');
                if (spl.Length <= 1)
                    continue;

                if (spl[0] == "add_field")
                {
                    Types.Add(spl[1], spl[2]);
                }
            }
        }
    }
}
