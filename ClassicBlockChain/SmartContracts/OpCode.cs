namespace UChainDB.Example.Chain.SmartContracts
{
    public enum OpCode : byte
    {
        Object,

        // ref: https://en.bitcoin.it/wiki/Script#Opcodes

        // constant
        PushFalse,
        Push0,
        PushTrue,
        Push1,
        Push1Negative,

        // flow
        Nop,
        If,
        NotIf,
        Else,
        EndIf,
        Verify,
        Return,

        // Stack
        ToAlternateStack,
        FromAlternateStack,
        IfDuplicated,
        Depth,
        Drop,
        Duplicate,
        // helpers:
        Nip,// remove second-to-top stack item
        Over,
        Pick,
        Roll,
        Rotate,
        Swap,
        Tuck,

        // splice
        Concatenate,
        SubString,
        Left,
        Right,
        Size,

        // bitwise
        Invert,
        And,
        Or,
        Xor,
        Equal,
        EqualVerify,

        // arithmetic
        Negate,
        Abs,
        Not,
        Add,
        Substract,
        Multiply,
        Divide,
        Modular,
        BooleanAnd,
        BooleanOr,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Min,
        Max,
        WithIn,

        // crypto
        RipeMd160,
        SHA1,
        SHA256,
        Hash160,
        Hash256,
        CheckSignature,
    }
}