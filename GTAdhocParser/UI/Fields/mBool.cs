﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syroot.BinaryData.Core;
using Syroot.BinaryData;
using Syroot.BinaryData.Memory;
using System.Diagnostics;

namespace GTAdhocTools.UI.Fields
{
    [DebuggerDisplay("mBool: {Name} ({Value})")]
    public class mBool : mTypeBase
    {
        public bool Value { get; set; }

        public override void Read(MBinaryIO io)
        {
            Value = io.Stream.ReadBoolean();
        }

        public override void WriteText(MTextWriter writer)
        {
            writer.WriteString(Name);
            writer.WriteSpace();
            writer.WriteString("digit");
            writer.WriteString("{"); writer.WriteString(Value ? "1" : "0"); writer.WriteString("}");
            writer.SetNeedNewLine();
        }
    }
}