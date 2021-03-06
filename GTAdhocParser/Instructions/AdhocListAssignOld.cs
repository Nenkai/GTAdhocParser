﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syroot.BinaryData.Memory;

namespace GTAdhocTools.Instructions
{
    public class OpListAssignOld : InstructionBase
    {
        public AdhocCallType CallType { get; set; } = AdhocCallType.LIST_ASSIGN_OLD;
        

        public uint Unk { get; set; }

        public override void Deserialize(AdhocFile parent, ref SpanReader sr)
        {
            Unk = sr.ReadUInt32();
        }

        public override string ToString()
           => $"{CallType}: Unk={Unk}";

        public override void Decompile(CodeBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
