using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Overlayer.Core.Utils;

namespace ErrorDetector
{
    public class UnityLogException
    {
        public Type ExceptionType { get; }
        public string Message { get; }
        public MethodBase TargetSite { get; }
        public MethodBase[] CallStack { get; }
        public string[] NotParsedCallStack { get; }
        public string Hash { get; }
        public UnityLogException(string condition, string stackTrace)
        {
            Hash = condition + stackTrace;
            if (!string.IsNullOrEmpty(condition))
            {
                var typeAndMessage = condition.Split2(':');
                ExceptionType = MiscUtils.TypeByName(typeAndMessage[0].Trim());
                Message = typeAndMessage[1].Trim();
            }
            if (!string.IsNullOrEmpty(stackTrace))
            {
                Queue<MethodBase> queue = new Queue<MethodBase>();
                Queue<string> notParsedQueue = new Queue<string>();
                using (StringReader sr = new StringReader(stackTrace))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        int offset = 0;
                        if (line.StartsWith("(")) offset += 2;
                        if (offset > 0) continue;
                        try
                        {
                            string[] methodArgs = line.Split(' ');
                            string method = methodArgs[0 + offset].Trim();
                            string[] methodSplit = method.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                            string methodDeclTypeName = string.Join(".", methodSplit.Take(methodSplit.Length - 1));
                            string methodFull = methodSplit[methodSplit.Length - 1];
                            string methodName = offset > 0 ? methodFull.GetBefore("(") : methodFull;
                            string methodParams;
                            if (offset > 0)
                            {
                                methodParams = methodFull.GetAfter("(");
                                methodParams = methodParams.Remove(methodParams.Length - 1);
                            }
                            else methodParams = methodArgs[1 + offset].Trim().GetBetween("(", ")");
                            Type methodDeclType = MiscUtils.TypeByName(methodDeclTypeName);
                            Type[] args =
                                string.IsNullOrWhiteSpace(methodParams) ? Type.EmptyTypes :
                                methodParams
                                .Split(',').Select(p => p.Trim().Split2(' '))
                                .Select(arr => MiscUtils.TypeByName(arr[0])).ToArray();
                            MethodBase methodBase;
                            if (methodName == "ctor")
                                methodBase = methodDeclType.GetConstructor((BindingFlags)15420, null, args, null);
                            else if (methodName == "cctor")
                                methodBase = methodDeclType.TypeInitializer;
                            else methodBase = methodDeclType.GetMethod(methodName, (BindingFlags)15420);
                            queue.Enqueue(methodBase);
                        }
                        catch (Exception ex)
                        {
                            Main.Logger.Log($"Parsing Error! ({ex.GetType()}: {ex.Message}) => {line}");
                            notParsedQueue.Enqueue(line);
                        }
                    }
                }
                CallStack = queue.ToArray();
                NotParsedCallStack = notParsedQueue.ToArray();
                TargetSite = CallStack[0];
            }
        }
        public override int GetHashCode() => Hash.GetHashCode();
    }
}
