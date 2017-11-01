﻿using System;
using System.Linq;
using System.Text;

namespace Neo.Compiler.MSIL
{
    /// <summary>
    /// 从ILCode 向小蚁 VM 转换的转换器
    /// </summary>
    public partial class ModuleConverter
    {
        private void _ConvertStLoc(ILMethod method, OpCode src, NeoMethod to, int pos)
        {

            //get array
            _Convert1by1(VM.OpCode.FROMALTSTACK, src, to);
            _Convert1by1(VM.OpCode.DUP, null, to);
            _Convert1by1(VM.OpCode.TOALTSTACK, null, to);
            //get i
            _ConvertPush(pos + method.paramtypes.Count, null, to);//翻转取参数顺序

            //getitem
            _ConvertPush(2, null, to);
            _Convert1by1(VM.OpCode.ROLL, null, to);

            _Convert1by1(VM.OpCode.SETITEM, null, to);


            //_Convert1by1(VM.OpCode.CLONESTRUCTONLY, src, to);
            ////push d
            //var c = _Convert1by1(VM.OpCode.DEPTH, null, to);
            //if (c.debugcode == null)
            //{
            //    c.debugcode = "from StLoc -> 6 code";
            //    c.debugline = 0;
            //}


            ////_Convert1by1(VM.ScriptOp.OP_DUP, src, to);
            ////push n
            //_ConvertPush(pos, null, to);
            ////d-n-1
            //_Convert1by1(VM.OpCode.SUB, null, to);
            //_Convert1by1(VM.OpCode.DEC, null, to);

            ////push olddepth
            //_Convert1by1(VM.OpCode.FROMALTSTACK, null, to);
            //_Convert1by1(VM.OpCode.DUP, null, to);
            //_Convert1by1(VM.OpCode.TOALTSTACK, null, to);
            ////(d-n-1)-olddepth
            //_Convert1by1(VM.OpCode.SUB, null, to);

            ////swap d-n-1 and top
            //_Convert1by1(VM.OpCode.XSWAP, null, to);
            ////drop top
            //_Convert1by1(VM.OpCode.DROP, null, to);

        }
        private void _ConvertLdLoc(ILMethod method, OpCode src, NeoMethod to, int pos)
        {
            //get array
            _Convert1by1(VM.OpCode.FROMALTSTACK, src, to);
            _Convert1by1(VM.OpCode.DUP, null, to);
            _Convert1by1(VM.OpCode.TOALTSTACK, null, to);
            //get i
            _ConvertPush(pos + method.paramtypes.Count, null, to);//翻转取参数顺序
            _Convert1by1(VM.OpCode.PICKITEM, null, to);


        }
        private void _ConvertLdLocA(ILMethod method, OpCode src, NeoMethod to, int pos)
        {//这有两种情况，我们需要先判断这个引用地址是拿出来干嘛的

            var n1 = method.body_Codes[method.GetNextCodeAddr(src.addr)];
            var n2 = method.body_Codes[method.GetNextCodeAddr(n1.addr)];
            if (n1.code == CodeEx.Initobj)//初始化结构体，必须给引用地址
            {
                _ConvertPush(pos + method.paramtypes.Count, src, to);
            }
            else if (n2.code == CodeEx.Call && n2.tokenMethod.Contains(".ctor"))
            {
                _ConvertPush(pos + method.paramtypes.Count, src, to);

            }
            else
            {
                _ConvertLdLoc(method, src, to, pos);
            }
        }
        private void _ConvertLdArg(OpCode src, NeoMethod to, int pos)
        {
            //get array
            _Convert1by1(VM.OpCode.FROMALTSTACK, src, to);
            _Convert1by1(VM.OpCode.DUP, null, to);
            _Convert1by1(VM.OpCode.TOALTSTACK, null, to);
            //get i
            _ConvertPush(pos, null, to);//翻转取参数顺序
            _Convert1by1(VM.OpCode.PICKITEM, null, to);

            ////push d
            //var c = _Convert1by1(VM.OpCode.DEPTH, src, to);
            //if (c.debugcode == null)
            //{
            //    c.debugcode = "from LdArg -> 5 code";
            //    c.debugline = 0;
            //}
            ////push n
            //_ConvertPush(pos, null, to);//翻转取参数顺序
            ////_Convert1by1(VM.OpCode.PUSHDATA1, null, to, int2Pushdata1bytes(to.paramtypes.Count - 1 - pos));
            ////d+n
            //_Convert1by1(VM.OpCode.ADD, null, to);

            ////push olddepth
            //_Convert1by1(VM.OpCode.FROMALTSTACK, null, to);
            //_Convert1by1(VM.OpCode.DUP, null, to);
            //_Convert1by1(VM.OpCode.TOALTSTACK, null, to);
            ////(d+n)-olddepth
            //_Convert1by1(VM.OpCode.SUB, null, to);

            ////pick
            //_Convert1by1(VM.OpCode.PICK, null, to);
        }
        private void _ConvertStArg(OpCode src, NeoMethod to, int pos)
        {
            //get array
            _Convert1by1(VM.OpCode.DUPFROMALTSTACK, src, to);
            //set i
            _ConvertPush(pos, null, to);//翻转取参数顺序

            //got v to top
            _ConvertPush(2, null, to);
            _Convert1by1(VM.OpCode.ROLL, null, to);

            _Convert1by1(VM.OpCode.SETITEM, null, to);
        }
        public bool IsSysCall(Mono.Cecil.MethodDefinition defs, out string name)
        {
            if (defs == null)
            {
                name = "";
                return false;
            }
            foreach (var attr in defs.CustomAttributes)
            {
                if (attr.AttributeType.Name == "SyscallAttribute")
                {
                    var type = attr.ConstructorArguments[0].Type;
                    var value = (string)attr.ConstructorArguments[0].Value;

                    //dosth
                    name = value;
                    return true;



                }
                //if(attr.t)
            }
            name = "";
            return false;


        }
        public bool IsEntryCall(Mono.Cecil.MethodDefinition defs,out byte id)
        {
            if (defs == null)
            {
                id = 0;
                return false;
            }
            foreach (var attr in defs.CustomAttributes)
            {
                if (attr.AttributeType.Name == "EntryPointAttribute")
                {
                    var type = attr.ConstructorArguments[0].Type;
                    var value = (byte)attr.ConstructorArguments[0].Value;

                    //dosth
                    id = value;
                    return true;



                }
                //if(attr.t)
            }
            id = 0;
            return false;

        }
        public bool IsAppCall(Mono.Cecil.MethodDefinition defs, out byte[] hash)
        {
            if (defs == null)
            {
                hash = null;
                return false;
            }
            foreach (var attr in defs.CustomAttributes)
            {
                if (attr.AttributeType.Name == "AppcallAttribute")
                {
                    var type = attr.ConstructorArguments[0].Type;
                    var a = attr.ConstructorArguments[0];
                    if (a.Type.FullName == "System.String")
                    {
                        string hashstr = (string)a.Value;

                        try
                        {
                            hash = new byte[20];
                            if (hashstr.Length < 40)
                                throw new Exception("hash too short:" + hashstr);
                            for (var i = 0; i < 20; i++)
                            {
                                hash[i] = byte.Parse(hashstr.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                            }
                            //string hexhash 需要反序
                            hash = hash.Reverse().ToArray();
                            return true;
                        }
                        catch
                        {
                            throw new Exception("hex format error:" + hashstr);
                        }
                    }
                    else
                    {
                        var list = a.Value as Mono.Cecil.CustomAttributeArgument[];

                        if (list == null || list.Length < 20)
                        {
                            throw new Exception("hash too short.");
                        }
                        hash = new byte[20];
                        for (var i = 0; i < 20; i++)
                        {
                            hash[i] = (byte)list[i].Value;
                        }
                        //byte hash 需要反序
                        hash = hash.Reverse().ToArray();
                        //dosth
                        return true;
                    }



                }
                //if(attr.t)
            }
            hash = null;
            return false;


        }
        public bool IsNonCall(Mono.Cecil.MethodDefinition defs)
        {
            if (defs == null)
            {
                return false;
            }
            foreach (var attr in defs.CustomAttributes)
            {
                if (attr.AttributeType.Name == "NonemitAttribute")
                {
                    return true;
                }
                //if(attr.t)
            }
            return false;
        }
        public bool IsOpCall(Mono.Cecil.MethodDefinition defs, out string name)
        {
            if (defs == null)
            {
                name = "";
                return false;
            }

            foreach (var attr in defs.CustomAttributes)
            {
                if (attr.AttributeType.Name == "OpCodeAttribute")
                {
                    var type = attr.ConstructorArguments[0].Type;
                    var value = (byte)attr.ConstructorArguments[0].Value;

                    foreach (var t in type.Resolve().Fields)
                    {
                        if (t.Constant != null)
                        {
                            if ((byte)t.Constant == value)
                            {

                                //dosth
                                name = t.Name;
                                return true;

                            }
                        }
                    }


                }
                //if(attr.t)
            }
            name = "";
            return false;

        }
        public bool IsNotifyCall(Mono.Cecil.MethodDefinition defs, Mono.Cecil.MethodReference refs, NeoMethod to, out string name)
        {
            name = to.lastsfieldname;
            Mono.Cecil.TypeDefinition call = null;
            if (defs == null)
            {
                try
                {
                    call = refs.DeclaringType.Resolve();
                }
                catch
                {//当不能取得这个，大半都是模板类

                }
            }
            else
            {
                call = defs.DeclaringType;
            }

            if (call != null)
            {
                if (call.BaseType.Name == "MulticastDelegate" || call.BaseType.Name == "Delegate")
                {
                    return true;
                }
            }
            else//不能还原类型，只好用名字判断了
            {
                if (refs.Name == "Invoke" && refs.DeclaringType.Name.Contains("Action`"))
                {
                    return true;

                }
            }
            name = "Notify";
            return false;
        }
        private int _ConvertCall(OpCode src, NeoMethod to)
        {
            Mono.Cecil.MethodReference refs = src.tokenUnknown as Mono.Cecil.MethodReference;

            int calltype = 0;
            string callname = "";
            byte[] callhash = null;
            VM.OpCode callcode = VM.OpCode.NOP;

            Mono.Cecil.MethodDefinition defs = null;
            try
            {
                defs = refs.Resolve();
            }
            catch
            {

            }

            if (IsNonCall(defs))
            {
                return 0;
            }
            else if (IsNotifyCall(defs, refs, to, out callname))
            {
                calltype = 5;
            }
            else if (IsOpCall(defs, out callname))
            {
                if (System.Enum.TryParse<VM.OpCode>(callname, out callcode))
                {
                    calltype = 2;
                }
                else
                {
                    throw new Exception("Can not find OpCall:" + callname);
                }
            }
            else if (IsSysCall(defs, out callname))
            {
                calltype = 3;
            }
            else if (IsAppCall(defs, out callhash))
            {
                calltype = 4;
            }
            else if (this.outModule.mapMethods.ContainsKey(src.tokenMethod))
            {//this is a call
                calltype = 1;
            }
            else
            {//maybe a syscall // or other
                if (src.tokenMethod.Contains("::op_Explicit(") || src.tokenMethod.Contains("::op_Implicit("))
                {
                    //各类显示隐示转换都忽略
                    //有可能有一些会特殊处理，故还保留独立判断
                    if (src.tokenMethod == "System.Int32 System.Numerics.BigInteger::op_Explicit(System.Numerics.BigInteger)")
                    {
                        //donothing
                        return 0;
                    }
                    else if (src.tokenMethod == "System.Int64 System.Numerics.BigInteger::op_Explicit(System.Numerics.BigInteger)")
                    {
                        //donothing
                        return 0;
                    }
                    else if (src.tokenMethod == "System.Numerics.BigInteger System.Numerics.BigInteger::op_Implicit(System.Int32)")//int->bignumber
                    {
                        //donothing
                        return 0;
                    }
                    else if (src.tokenMethod == "System.Numerics.BigInteger System.Numerics.BigInteger::op_Implicit(System.Int64)")
                    {
                        return 0;
                    }

                    return 0;
                }
                else if (src.tokenMethod == "System.Void System.Diagnostics.Debugger::Break()")
                {
                    _Convert1by1(VM.OpCode.NOP, src, to);

                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_Equality(") || src.tokenMethod.Contains("::Equals("))
                {
                    var _ref = src.tokenUnknown as Mono.Cecil.MethodReference;

                    if (_ref.DeclaringType.FullName == "System.Boolean"
                        || _ref.DeclaringType.FullName == "System.Int32"
                        || _ref.DeclaringType.FullName == "System.Numerics.BigInteger")
                    {
                        _Convert1by1(VM.OpCode.NUMEQUAL, src, to);
                    }
                    else
                    {
                        _Convert1by1(VM.OpCode.EQUAL, src, to);

                    }
                    //各类==指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    //if (src.tokenMethod == "System.Boolean System.String::op_Equality(System.String,System.String)")
                    //{
                    //    _Convert1by1(VM.OpCode.EQUAL, src, to);
                    //    return 0;
                    //}
                    //else if (src.tokenMethod == "System.Boolean System.Object::Equals(System.Object)")
                    //{
                    //    _Convert1by1(VM.OpCode.EQUAL, src, to);
                    //    return 0;
                    //}
                    //_Convert1by1(VM.OpCode.EQUAL, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_Inequality("))
                {
                    var _ref = src.tokenUnknown as Mono.Cecil.MethodReference;
                    if (_ref.DeclaringType.FullName == "System.Boolean"
                        || _ref.DeclaringType.FullName == "System.Int32"
                        || _ref.DeclaringType.FullName == "System.Numerics.BigInteger")
                    {
                        _Convert1by1(VM.OpCode.NUMNOTEQUAL, src, to);
                    }
                    else
                    {
                        _Convert1by1(VM.OpCode.INVERT, src, to);
                        _Insert1(VM.OpCode.EQUAL, "", to);
                    }
                    ////各类!=指令
                    ////有可能有一些会特殊处理，故还保留独立判断
                    //if (src.tokenMethod == "System.Boolean System.Numerics.BigInteger::op_Inequality(System.Numerics.BigInteger,System.Numerics.BigInteger)")
                    //{
                    //    _Convert1by1(VM.OpCode.INVERT, src, to);
                    //    _Insert1(VM.OpCode.EQUAL, "", to);
                    //    return 0;
                    //}
                    //_Convert1by1(VM.OpCode.INVERT, src, to);
                    //_Insert1(VM.OpCode.EQUAL, "", to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_Addition("))
                {
                    //各类+指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    if (src.tokenMethod == "System.Numerics.BigInteger System.Numerics.BigInteger::op_Addition(System.Numerics.BigInteger,System.Numerics.BigInteger)")
                    {
                        _Convert1by1(VM.OpCode.ADD, src, to);
                        return 0;
                    }
                    _Convert1by1(VM.OpCode.ADD, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_Subtraction("))
                {
                    //各类-指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    if (src.tokenMethod == "System.Numerics.BigInteger System.Numerics.BigInteger::op_Subtraction(System.Numerics.BigInteger,System.Numerics.BigInteger)")
                    {
                        _Convert1by1(VM.OpCode.SUB, src, to);
                        return 0;
                    }
                    _Convert1by1(VM.OpCode.SUB, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_Multiply("))
                {
                    //各类*指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    if (src.tokenMethod == "System.Numerics.BigInteger System.Numerics.BigInteger::op_Multiply(System.Numerics.BigInteger,System.Numerics.BigInteger)")
                    {
                        _Convert1by1(VM.OpCode.MUL, src, to);
                        return 0;
                    }
                    _Convert1by1(VM.OpCode.MUL, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_Division("))
                {
                    //各类/指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    if (src.tokenMethod == "System.Numerics.BigInteger System.Numerics.BigInteger::op_Division(System.Numerics.BigInteger, System.Numerics.BigInteger)")
                    {
                        _Convert1by1(VM.OpCode.DIV, src, to);
                        return 0;
                    }
                    _Convert1by1(VM.OpCode.DIV, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_Modulus("))
                {
                    //各类%指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    if (src.tokenMethod == "System.Numerics.BigInteger System.Numerics.BigInteger::op_Modulus(System.Numerics.BigInteger,System.Numerics.BigInteger)")
                    {
                        _Convert1by1(VM.OpCode.MOD, src, to);
                        return 0;
                    }
                    _Convert1by1(VM.OpCode.MOD, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_LessThan("))
                {
                    //各类<指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    _Convert1by1(VM.OpCode.LT, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_GreaterThan("))
                {
                    //各类>指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    _Convert1by1(VM.OpCode.GT, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_LessThanOrEqual("))
                {
                    //各类<=指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    _Convert1by1(VM.OpCode.LTE, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::op_GreaterThanOrEqual("))
                {
                    //各类>=指令
                    //有可能有一些会特殊处理，故还保留独立判断
                    _Convert1by1(VM.OpCode.GTE, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::get_Length("))
                {
                    //各类.Length指令
                    //"System.Int32 System.String::get_Length()"
                    _Convert1by1(VM.OpCode.SIZE, src, to);
                    return 0;
                }
                else if (src.tokenMethod.Contains("::Concat("))
                {
                    //各类.Concat
                    //"System.String System.String::Concat(System.String,System.String)"
                    _Convert1by1(VM.OpCode.CAT, src, to);
                    return 0;
                }

                else if (src.tokenMethod == "System.String System.String::Substring(System.Int32,System.Int32)")
                {
                    _Convert1by1(VM.OpCode.SUBSTR, src, to);
                    return 0;

                }
                else if (src.tokenMethod == "System.Char System.String::get_Chars(System.Int32)")
                {
                    _ConvertPush(1, src, to);
                    _Convert1by1(VM.OpCode.SUBSTR, null, to);
                    return 0;
                }
                else if (src.tokenMethod == "System.String System.String::Substring(System.Int32)")
                {
                    throw new Exception("neomachine cant use this call,please use  .SubString(1,2) with 2 params.");
                }
                else if (src.tokenMethod == "System.String System.Char::ToString()")
                {
                    return 0;
                }
                else if (src.tokenMethod == "System.Byte[] System.Numerics.BigInteger::ToByteArray()")
                {
                    return 0;
                }
                else if (src.tokenMethod == "System.Void System.Numerics.BigInteger::.ctor(System.Byte[])")
                {
                    _Convert1by1(VM.OpCode.DUPFROMALTSTACK, src, to);
                    _ConvertPush(2, null, to);
                    _Convert1by1(VM.OpCode.ROLL, null, to);
                    _ConvertPush(2, null, to);
                    _Convert1by1(VM.OpCode.ROLL, null, to);
                    _Convert1by1(VM.OpCode.SETITEM, null, to);
                    return 0;
                }
                else if (src.tokenMethod == "System.UInt32 <PrivateImplementationDetails>::ComputeStringHash(System.String)")
                {
                    throw new Exception("需要neo.vm nuget更新以后，这个才可以放开，就可以处理 string switch了。");
                    //_Convert1by1(VM.OpCode.CSHARPSTRHASH32, src, to);
                    //return 0;
                }
                else
                {

                }
            }

            if (calltype == 0)
                throw new Exception("unknown call: " + src.tokenMethod + "\r\n   in: " + to.name + "\r\n");
            var md = src.tokenUnknown as Mono.Cecil.MethodReference;
            var pcount = md.Parameters.Count;
            bool havethis = md.HasThis;
            if(calltype==2)
            {
                //opcode call 
            }
            else
            {//翻转参数顺序

                //如果是syscall 并且有this的，翻转范围加一
                if (calltype == 3 && havethis)
                    pcount++;

                _Convert1by1(VM.OpCode.NOP, src, to);
                if (pcount <= 1)
                {
                }
                else if (pcount == 2)
                {
                    _Insert1(VM.OpCode.SWAP, "swap 2 param", to);
                }
                else if (pcount == 3)
                {
                    _InsertPush(2, "swap 0 and 2 param", to);
                    _Insert1(VM.OpCode.XSWAP, "", to);
                }
                else
                {
                    for (var i = 0; i < pcount / 2; i++)
                    {
                        int saveto = (pcount - 1 - i);
                        _InsertPush(saveto, "load" + saveto, to);
                        _Insert1(VM.OpCode.PICK, "", to);

                        _InsertPush(i + 1, "load" + i + 1, to);
                        _Insert1(VM.OpCode.PICK, "", to);


                        _InsertPush(saveto + 2, "save to" + saveto + 2, to);
                        _Insert1(VM.OpCode.XSWAP, "", to);
                        _Insert1(VM.OpCode.DROP, "", to);

                        _InsertPush(i + 1, "save to" + i + 1, to);
                        _Insert1(VM.OpCode.XSWAP, "", to);
                        _Insert1(VM.OpCode.DROP, "", to);

                    }
                }
            }
            if (calltype == 1)
            {
                var c = _Convert1by1(VM.OpCode.CALL, null, to, new byte[] { 5, 0 });
                c.needfixfunc = true;
                c.srcfunc = src.tokenMethod;
                return 0;
            }
            else if (calltype == 2)
            {
                _Convert1by1(callcode, null, to);
                return 0;
            }
            else if (calltype == 3)
            {
                var bytes = Encoding.UTF8.GetBytes(callname);
                if (bytes.Length > 252) throw new Exception("string is to long");
                byte[] outbytes = new byte[bytes.Length + 1];
                outbytes[0] = (byte)bytes.Length;
                Array.Copy(bytes, 0, outbytes, 1, bytes.Length);
                //bytes.Prepend 函数在 dotnet framework 4.6 编译不过
                _Convert1by1(VM.OpCode.SYSCALL, null, to, outbytes);
                return 0;
            }
            else if (calltype == 4)
            {
                _Convert1by1(VM.OpCode.APPCALL, null, to, callhash);

            }
            else if (calltype == 5)
            {

                //把name参数推进去
                var callp = Encoding.UTF8.GetBytes(callname);
                _ConvertPush(callp, src, to);

                //参数打包成array
                _ConvertPush(pcount + 1, null, to);
                _Convert1by1(VM.OpCode.PACK, null, to);

                //a syscall
                {
                    var bytes = Encoding.UTF8.GetBytes("Neo.Runtime.Notify");
                    byte[] outbytes = new byte[bytes.Length + 1];
                    outbytes[0] = (byte)bytes.Length;
                    Array.Copy(bytes, 0, outbytes, 1, bytes.Length);
                    //bytes.Prepend 函数在 dotnet framework 4.6 编译不过
                    _Convert1by1(VM.OpCode.SYSCALL, null, to, outbytes);
                }
            }
            return 0;
        }

        private int _ConvertNewArr(ILMethod method, OpCode src, NeoMethod to)
        {
            var type = src.tokenType;
            if (type != "System.Byte")
            {
                _Convert1by1(VM.OpCode.NEWARRAY, src, to);
                int n = method.GetNextCodeAddr(src.addr);
                int n2 = method.GetNextCodeAddr(n);
                int n3 = method.GetNextCodeAddr(n2);
                if (n >= 0 && n2 >= 0 && n3 >= 0 && method.body_Codes[n].code == CodeEx.Dup && method.body_Codes[n2].code == CodeEx.Ldtoken && method.body_Codes[n3].code == CodeEx.Call)
                {//這是在初始化數組
                    var data = method.body_Codes[n2].tokenUnknown as byte[];
                    if (type == "System.Char")
                    {
                        for (var i = 0; i < data.Length; i += 2)
                        {
                            char info = BitConverter.ToChar(data, i);
                            _Convert1by1(VM.OpCode.DUP, null, to);
                            _ConvertPush(i / 2, null, to);
                            _ConvertPush(info, null, to);
                            _Convert1by1(VM.OpCode.SETITEM, null, to);
                        }
                        return 3;
                    }
                    throw new Exception("not support this type's init array.");

                }
                return 0;
                //this.logger.Log("_ConvertNewArr::not support type " + type + " for array.");
            }
            else
            {
                var code = to.body_Codes.Last().Value;
                //we need a number
                if (code.code > VM.OpCode.PUSH16)
                {
                    throw new Exception("_ConvertNewArr::not support var lens for new byte[?].");
                }
                var number = getNumber(code);

                //移除上一条指令
                to.body_Codes.Remove(code.addr);
                this.addr = code.addr;
                if (code.bytes != null)
                    this.addr -= code.bytes.Length;

                int n = method.GetNextCodeAddr(src.addr);
                int n2 = method.GetNextCodeAddr(n);
                int n3 = method.GetNextCodeAddr(n2);
                if (n >= 0 && n2 >= 0 && n3 >= 0 && method.body_Codes[n].code == CodeEx.Dup && method.body_Codes[n2].code == CodeEx.Ldtoken && method.body_Codes[n3].code == CodeEx.Call)
                {//這是在初始化數組

                    var data = method.body_Codes[n2].tokenUnknown as byte[];
                    this._ConvertPush(data, src, to);

                    return 3;

                }
                else
                {
                    this._ConvertPush(new byte[number], src, to);
                }
            }



            return 0;

        }
        private int _ConvertInitObj(OpCode src, NeoMethod to)
        {
            var type = (src.tokenUnknown as Mono.Cecil.TypeReference).Resolve();
            _Convert1by1(VM.OpCode.NOP, src, to);//空白
            _ConvertPush(type.Fields.Count, null, to);//插入个数量
            if (type.IsValueType)
            {
                _Insert1(VM.OpCode.NEWSTRUCT, null, to);
            }
            else
            {
                _Insert1(VM.OpCode.NEWARRAY, null, to);
            }
            //now stack  a index, a value

            //getarray
            _Insert1(VM.OpCode.FROMALTSTACK, null, to);
            _Insert1(VM.OpCode.DUP, null, to);
            _Insert1(VM.OpCode.TOALTSTACK, null, to);

            _InsertPush(2, "", to);//move item
            _Insert1(VM.OpCode.ROLL, null, to);

            _InsertPush(2, "", to);//move value
            _Insert1(VM.OpCode.ROLL, null, to);

            _Insert1(VM.OpCode.SETITEM, null, to);

            ////然後要將計算棧上的第一個值，寫入第二個值對應的pos
            //_Convert1by1(VM.OpCode.SWAP, null, to);//replace n to top

            ////push d
            //_Convert1by1(VM.OpCode.DEPTH, null, to);

            //_Convert1by1(VM.OpCode.DEC, null, to);//d 多了一位，剪掉
            //_Convert1by1(VM.OpCode.SWAP, null, to);//把n拿上來
            ////push n
            ////_ConvertPush(pos, null, to);有n了
            ////d-n-1
            //_Convert1by1(VM.OpCode.SUB, null, to);
            //_Convert1by1(VM.OpCode.DEC, null, to);

            ////push olddepth
            //_Convert1by1(VM.OpCode.FROMALTSTACK, null, to);
            //_Convert1by1(VM.OpCode.DUP, null, to);
            //_Convert1by1(VM.OpCode.TOALTSTACK, null, to);
            ////(d-n-1)-olddepth
            //_Convert1by1(VM.OpCode.SUB, null, to);

            ////swap d-n-1 and top
            //_Convert1by1(VM.OpCode.XSWAP, null, to);
            ////drop top
            //_Convert1by1(VM.OpCode.DROP, null, to);
            return 0;
        }
        private int _ConvertNewObj(OpCode src, NeoMethod to)
        {
            var _type = (src.tokenUnknown as Mono.Cecil.MethodReference);
            if (_type.FullName == "System.Void System.Numerics.BigInteger::.ctor(System.Byte[])")
            {
                return 0;//donothing;

            }
            else if (_type.DeclaringType.FullName.Contains("Exception"))
            {
                _Convert1by1(VM.OpCode.NOP, src, to);//空白
                var pcount = _type.Parameters.Count;
                for (var i = 0; i < pcount; i++)
                {
                    _Insert1(VM.OpCode.DROP, "", to);
                }
                return 0;
            }
            var type = _type.Resolve();
            _Convert1by1(VM.OpCode.NOP, src, to);//空白
            _ConvertPush(type.DeclaringType.Fields.Count, null, to);//插入个数量
            if (type.DeclaringType.IsValueType)
            {
                _Insert1(VM.OpCode.NEWSTRUCT, null, to);
            }
            else
            {
                _Insert1(VM.OpCode.NEWARRAY, null, to);
            }
            return 0;
        }

        private int _ConvertStfld(ILMethod method, OpCode src, NeoMethod to)
        {
            var field = (src.tokenUnknown as Mono.Cecil.FieldReference).Resolve();
            var type = field.DeclaringType;
            var id = type.Fields.IndexOf(field);
            if (id < 0)
                throw new Exception("impossible.");

            //_Convert1by1(VM.OpCode.CLONESTRUCTONLY, src, to);

            _ConvertPush(id, null, to);//index
            _Convert1by1(VM.OpCode.SWAP, null, to);//把item 拿上來 

            _Convert1by1(VM.OpCode.SETITEM, null, to);//修改值 //item //index //array
            return 0;
        }

        private int _ConvertLdfld(OpCode src, NeoMethod to)
        {
            var field = (src.tokenUnknown as Mono.Cecil.FieldReference).Resolve();
            var type = field.DeclaringType;
            var id = type.Fields.IndexOf(field);
            if (id < 0)
                throw new Exception("impossible.");
            _ConvertPush(id, src, to);
            _Convert1by1(VM.OpCode.PICKITEM, null, to);//修改值

            return 0;
        }
    }
}
