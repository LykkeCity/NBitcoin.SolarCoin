using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NBitcoin.SolarCoin.Extensions
{
    public static class ScriptExtensions
    {
        public static Script FindAndDelete(this Script script, OpcodeType op)
        {
            var opInsance = (Op)typeof(Op).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null).Invoke(null);
            opInsance.Code = op;

            return script.FindAndDelete(opInsance);
        }

        public static Script FindAndDelete(this Script script, Op op)
        {
            return op == null ? script : script.FindAndDelete(o => o.Code == op.Code && Utils.ArrayEqual(o.PushData, op.PushData));
        }

        public static Script FindAndDelete(this Script script, Func<Op, bool> predicate)
        {
            int nFound = 0;
            List<Op> operations = new List<Op>();
            foreach (var op in script.ToOps())
            {
                var shouldDelete = predicate(op);
                if (!shouldDelete)
                {
                    operations.Add(op);
                }
                else
                    nFound++;
            }
            if (nFound == 0)
                return script;
            return new Script(operations);
        }

        public static IEnumerable<Op> ToOps(this Script script)
        {
            ScriptReader reader = new ScriptReader(script.ToBytes());
            return reader.ToEnumerable();
        }
    }
}
