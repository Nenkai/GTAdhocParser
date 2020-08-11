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
    public class AdhocCode : InstructionBase
    {
        private byte[] _buffer;
        public AdhocCallType CallType { get; set; } = AdhocCallType.METHOD_DEFINE;

        public List<InstructionBase> Components = new List<InstructionBase>();

        /// <summary>
        /// Arguments if the current component is a function call.
        /// The first argument is always "self", if declaring a class method.
        /// </summary>
        public List<(string argumentName, uint argumentIndex)> Arguments = new List<(string, uint)>();
        public List<string> unkStr2 = new List<string>();

        public string OriginalSourceFile { get; set; }

        public override void Deserialize(AdhocFile parent, ref SpanReader sr)
        {
            sr.ReadByte();
            uint dataVersion = sr.ReadByte();

            uint fileNameIndex = (uint)sr.DecodeBitsAndAdvance(); // vers > 9
            OriginalSourceFile = parent.StringTable[fileNameIndex];

            byte unk = sr.ReadByte(); // vers > 12
            uint argCount = sr.ReadUInt32();

            if (argCount > 0)
            {
                for (int i = 0; i < argCount; i++)
                {
                    string argName = Utils.ReadADCString(parent, ref sr);
                    Arguments.Add((argName, sr.ReadUInt32()));
                }
            }

            uint unkCount2 = sr.ReadUInt32();
            if (unkCount2 > 0)
            {
                for (int i = 0; i < unkCount2; i++)
                {
                    string u = Utils.ReadADCString(parent, ref sr);
                    unkStr2.Add(u);
                    sr.ReadUInt32();
                }
            }

            uint unkCount3 = sr.ReadUInt32();

            /*
            if ((int)codeStream->adcVersionCurrent < 0xb)
            {
                ReadInt32(codeStream, uVar5 + 0x34 & 0xffffffff);
                ReadInt32(codeStream, uVar5 + 0x30 & 0xffffffff);
                param_1->field_0x38 = param_1->field_0x34;
            }
            else
            {
            */

            uint unkCount4 = sr.ReadUInt32();
            uint unkCount5 = sr.ReadUInt32();
            uint unkCount6 = sr.ReadUInt32();

            uint instructionCount = sr.ReadUInt32();
            if (instructionCount < 0x40000000)
            {
                for (int i = 0; i < instructionCount; i++)
                {
                    uint idk = sr.ReadUInt32();
                    AdhocCallType type = (AdhocCallType)sr.ReadByte();

                    ReadComponent(parent, idk, type, ref sr);
                }
            }
        }

        public void ReadComponent(AdhocFile parent, uint lineNumber, AdhocCallType type, ref SpanReader sr)
        {
            InstructionBase component = GetByType(type);
            if (component != null)
            {
                component.InstructionOffset = (uint)sr.Position - 5;
                component.LineNumber = lineNumber;
                component.Deserialize(parent, ref sr);
                Components.Add(component);
            }
        }

        public static InstructionBase GetByType(AdhocCallType type)
        {
            switch (type)
            {
                case AdhocCallType.MODULE_DEFINE:
                    return new OpModule();
                case AdhocCallType.METHOD_DEFINE:
                case AdhocCallType.FUNCTION_DEFINE:
                case AdhocCallType.FUNCTION_CONST:
                case AdhocCallType.METHOD_CONST:
                    return new OpMethod(type);
                case AdhocCallType.VARIABLE_EVAL:
                    return new OpVariableEval();
                case AdhocCallType.CALL:
                    return new OpCall();
                case AdhocCallType.JUMP_IF_FALSE:
                    return new OpJumpIfFalse();
                case AdhocCallType.FLOAT_CONST:
                    return new OpFloatConst();
                case AdhocCallType.ATTRIBUTE_PUSH:
                    return new OpAttributePush();
                case AdhocCallType.ASSIGN_POP:
                    return new OpAssignPop(); 
                case AdhocCallType.LEAVE:
                    return new OpLeave();
                case AdhocCallType.VOID_CONST:
                    return new OpVoidConst();
                case AdhocCallType.SET_STATE:
                    return new OpSetState();
                case AdhocCallType.NIL_CONST:
                    return new OpNilConst();
                case AdhocCallType.ATTRIBUTE_DEFINE:
                    return new OpAttributeDefine();
                case AdhocCallType.BOOL_CONST:
                    return new OpBoolConst();
                case AdhocCallType.SOURCE_FILE:
                    return new OpSourceFile();
                case AdhocCallType.IMPORT:
                    return new OpImport();
                case AdhocCallType.STRING_CONST:
                    return new OpStringConst();
                case AdhocCallType.POP:
                    return new OpPop();
                case AdhocCallType.CLASS_DEFINE:
                    return new OpClassDefine();
                case AdhocCallType.ATTRIBUTE_EVAL:
                    return new OpAttributeEval();
                case AdhocCallType.INT_CONST:
                    return new OpIntConst();
                case AdhocCallType.STATIC_DEFINE:
                    return new OpStaticDefine();
                case AdhocCallType.VARIABLE_PUSH:
                    return new OpVariablePush();
                case AdhocCallType.BINARY_OPERATOR:
                    return new OpBinaryOperator();
                case AdhocCallType.JUMP:
                    return new OpJump();
                case AdhocCallType.ELEMENT_EVAL:
                    return new OpElementEval();
                case AdhocCallType.STRING_PUSH:
                    return new OpStringPush();
                case AdhocCallType.JUMP_IF_TRUE:
                    return new OpJumpIfTrue();
                case AdhocCallType.EVAL:
                    return new OpEval();
                case AdhocCallType.BINARY_ASSIGN_OPERATOR:
                    return new OpBinaryAssignOperator();
                case AdhocCallType.LOGICAL_OR:
                    return new OpLogicalOr();
                case AdhocCallType.LIST_ASSIGN:
                    return new OpListAssign();
                case AdhocCallType.ELEMENT_PUSH:
                    return new OpElementPush();
                case AdhocCallType.MAP_CONST:
                    return new OpMapConst();
                case AdhocCallType.MAP_INSERT:
                    return new OpMapInsert();
                case AdhocCallType.UNARY_OPERATOR:
                    return new OpUnaryOperator();
                case AdhocCallType.LOGICAL_AND:
                    return new OpLogicalAnd();
                case AdhocCallType.ARRAY_CONST:
                    return new OpArrayConst();
                case AdhocCallType.ARRAY_PUSH:
                    return new OpArrayPush();
                case AdhocCallType.UNARY_ASSIGN_OPERATOR:
                    return new OpUnaryAssignOperator();
                case AdhocCallType.SYMBOL_CONST:
                    return new OpSymbolConst();
                case AdhocCallType.OBJECT_SELECTOR:
                    return new OpObjectSelector();
                case AdhocCallType.LONG_CONST:
                    return new OpLongConst();
                case AdhocCallType.UNDEF:
                    return new OpUndef();
                case AdhocCallType.TRY_CATCH:
                    return new OpTryCatch();
                case AdhocCallType.ASSIGN:
                    return new OpAssign();
                default:
                    return null;
            }
        }

        public void Decompile(CodeBuilder builder)
        {
            builder.AppendLine(string.Empty);
        }
    }

    public enum AdhocCallType
    {
        ARRAY_CONST_OLD,
        ASSIGN_OLD,
        ATTRIBUTE_DEFINE,
        ATTRIBUTE_PUSH,
        BINARY_ASSIGN_OPERATOR,
        BINARY_OPERATOR,
        CALL,
        CLASS_DEFINE,
        EVAL,
        FLOAT_CONST,
        FUNCTION_DEFINE,
        IMPORT,
        INT_CONST,
        JUMP,
        JUMP_IF_TRUE,
        JUMP_IF_FALSE,
        LIST_ASSIGN_OLD,
        LOCAL_DEFINE,
        LOGICAL_AND_OLD,
        LOGICAL_OR_OLD,
        METHOD_DEFINE,
        MODULE_DEFINE,
        NIL_CONST,
        NOP,
        POP_OLD,
        PRINT,
        REQUIRE,
        SET_STATE_OLD,
        STATIC_DEFINE,
        STRING_CONST,
        STRING_PUSH,
        THROW,
        TRY_CATCH,
        UNARY_ASSIGN_OPERATOR,
        UNARY_OPERATOR,
        UNDEF,
        VARIABLE_PUSH,
        ATTRIBUTE_EVAL,
        VARIABLE_EVAL,
        SOURCE_FILE,
        FUNCTION_CONST,
        METHOD_CONST,
        MAP_CONST_OLD,
        LONG_CONST,
        ASSIGN,
        LIST_ASSIGN,
        CALL_OLD,
        OBJECT_SELECTOR,
        SYMBOL_CONST,
        LEAVE,
        ARRAY_CONST,
        ARRAY_PUSH,
        MAP_CONST,
        MAP_INSERT,
        POP,
        SET_STATE,
        VOID_CONST,
        ASSIGN_POP,
        U_INT_CONST,
        U_LONG_CONST,
        DOUBLE_CONST,
        ELEMENT_PUSH,
        ELEMENT_EVAL,
        LOGICAL_AND,
        LOGICAL_OR,
        BOOL_CONST,
        MODULE_CONSTRUCTOR,
        VA_CALL,
        CODE_EVAL,

    }
}
