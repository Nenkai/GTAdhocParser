﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Syroot.BinaryData.Memory;

using GTAdhocParser.Instructions;

namespace GTAdhocParser
{
    public class AdhocFile
    {
        public const string MAGIC = "ADCH";
        public string[] StringTable { get; private set; }
        public byte[] _buffer;

        public AdhocCode ParentCode { get; set; }
        public byte Version { get; set; }

        private AdhocFile(byte version)
        {
            Version = version;
        }

        public static AdhocFile ReadFromFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var sr = new SpanReader(bytes, encoding: Encoding.ASCII);

            string magic = sr.ReadStringRaw(4);
            if (!magic.Equals(MAGIC))
                throw new Exception("Invalid MAGIC, doesn't match ADCH.");

            byte version = (byte)int.Parse(Encoding.ASCII.GetChars(sr.ReadBytes(3)));
            sr.ReadByte();
            var adc = new AdhocFile(version);

            if (adc.Version >= 8)
                adc.ParseStringTable(ref sr);

            adc.ParentCode = new AdhocCode();
            adc.ParentCode.Deserialize(adc, ref sr);

            return adc;
        }

        public void ParseStringTable(ref SpanReader sr)
        {
            uint entryCount = (uint)sr.DecodeBitsAndAdvance();
            StringTable = new string[entryCount];
            for (var i = 0; i < entryCount; i++)
            {
                int strLen = (int)sr.DecodeBitsAndAdvance();
                StringTable[i] = sr.ReadStringRaw(strLen);
            }
        }

        public void PrintStringTable(string outPath)
        {
            var sr = new SpanReader(_buffer);
            sr.Position = 8;

            using var sw = new StreamWriter(outPath);
            uint entryCount = (uint)sr.DecodeBitsAndAdvance();
            var results = new string[entryCount];
            for (var i = 0; i < entryCount; i++)
            {
                sw.WriteLine($"0x{sr.Position:X2} | {sr.ReadString1()}");
            }
            sw.Flush();
        }

        public void Decompile(string outPath)
        {
            using var sw = new StreamWriter(outPath);
            sw.WriteLine("==== Disassembly generated by GTAdhocParser by Nenkai#9075 ====");
            if (!string.IsNullOrEmpty(ParentCode.OriginalSourceFile))
                sw.WriteLine($"Original File Name: {ParentCode.OriginalSourceFile}");

            sw.Write($"Version: {Version}");
            if (StringTable != null)
                sw.Write($"{StringTable.Length} strings)");
            sw.WriteLine();

            var d = new CodeBuilder();
            for (var i = 0; i < ParentCode.Components.Count; i++)
            {
                var inst = ParentCode.Components[i];
                inst.Decompile(d);
            }

            sw.Flush();
        }

        public void Disassemble(string outPath, bool withOffset)
        {
            using var sw = new StreamWriter(outPath);
            sw.WriteLine("==== Disassembly generated by GTAdhocParser by Nenkai#9075 ====");
            if (!string.IsNullOrEmpty(ParentCode.OriginalSourceFile))
                sw.WriteLine($"Original File Name: {ParentCode.OriginalSourceFile}");

            sw.Write($"Version: {Version}");
            if (StringTable != null)
                sw.Write($"{StringTable.Length} strings)");
            sw.WriteLine();

            var d = new CodeBuilder();

            int ifdepth = 0;
            for (var i = 0; i < ParentCode.Components.Count; i++)
            {
                var inst = ParentCode.Components[i];

                if (inst is OpMethod)
                    sw.WriteLine();

                if (ifdepth > 0)
                    sw.Write(new string(' ', 2 * ifdepth));

                if (withOffset)
                    sw.Write($"{inst.InstructionOffset - 5,6:X2}|");
                sw.Write($"{inst.SourceLineNumber,4}|");
                sw.Write($"{i,3}| "); // Function Instruction Number
                sw.WriteLine(ParentCode.Components[i]);

                int depth = 0;
                if (inst is OpMethod method)
                    DisassembleMethod(sw, method, withOffset, ref depth);
                else if (inst is OpJumpIfFalse || inst is OpJumpIfTrue)
                    ifdepth++;
                else if (inst is OpLeave)
                    ifdepth--;
            }

            sw.Flush();
        }

        public void DisassembleMethod(StreamWriter sw, OpMethod method, bool withOffset, ref int depth)
        {
            depth++;

            int ifdepth = 0;
            string curDepthStr = new string(' ', 2 * depth);
            for (int i = 0; i < method.Code.Components.Count; i++)
            {
                InstructionBase metInstruction = method.Code.Components[i];
                sw.Write(curDepthStr);

                if (ifdepth > 0)
                    sw.Write(new string(' ', 2 * ifdepth));

                if (withOffset)
                    sw.Write($"{metInstruction.InstructionOffset - 5,6:X2}|");
                sw.Write($"{metInstruction.SourceLineNumber,4}|");
                sw.Write($"{i,3}| "); // Function Instruction Number
                sw.WriteLine(metInstruction);

                if (metInstruction is OpMethod methodFunction)
                    DisassembleMethod(sw, methodFunction, withOffset, ref depth);
                else if (metInstruction is OpJumpIfFalse || metInstruction is OpJumpIfTrue)
                    ifdepth++;
                else if (metInstruction is OpLeave)
                    ifdepth--;
            }

            depth--;
            sw.WriteLine();
        }
    }
}
