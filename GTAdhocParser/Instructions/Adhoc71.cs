﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syroot.BinaryData.Memory;

namespace GTAdhocParser.Instructions
{
    public class Op71 : InstructionBase
    {
        public AdhocCallType CallType { get; set; } = AdhocCallType.UNK_71;

        public int Value { get; set; }
        public override void Deserialize(AdhocFile parent, ref SpanReader sr)
        {
            
        }

        public override string ToString()
            => CallType.ToString();

        public override void Decompile(CodeBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}