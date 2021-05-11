using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.RemoteCalls
{
    // command function delegate
    public delegate void CmdDelegate(NetworkBehaviour obj, NetworkReader reader,
        NetworkConnectionToClient senderConnection);

    internal class Invoker
    {
        public bool cmdRequiresAuthority;
        public Type invokeClass;
        public CmdDelegate invokeFunction;
        public MirrorInvokeType invokeType;

        public bool AreEqual(Type invokeClass, MirrorInvokeType invokeType, CmdDelegate invokeFunction)
        {
            return this.invokeClass == invokeClass &&
                   this.invokeType == invokeType &&
                   this.invokeFunction == invokeFunction;
        }
    }

    public struct CommandInfo
    {
        public bool requiresAuthority;
    }

    /// <summary>Used to help manage remote calls for NetworkBehaviours</summary>
    public static class RemoteCallHelper
    {
        private static readonly Dictionary<int, Invoker> cmdHandlerDelegates = new Dictionary<int, Invoker>();

        internal static int GetMethodHash(Type invokeClass, string methodName)
        {
            // (invokeClass + ":" + cmdName).GetStableHashCode() would cause allocations.
            // so hash1 + hash2 is better.
            unchecked
            {
                var hash = invokeClass.FullName.GetStableHashCode();
                return hash * 503 + methodName.GetStableHashCode();
            }
        }

        internal static int RegisterDelegate(Type invokeClass, string cmdName, MirrorInvokeType invokerType,
            CmdDelegate func, bool cmdRequiresAuthority = true)
        {
            // type+func so Inventory.RpcUse != Equipment.RpcUse
            var cmdHash = GetMethodHash(invokeClass, cmdName);

            if (CheckIfDeligateExists(invokeClass, invokerType, func, cmdHash))
                return cmdHash;

            var invoker = new Invoker
            {
                invokeType = invokerType,
                invokeClass = invokeClass,
                invokeFunction = func,
                cmdRequiresAuthority = cmdRequiresAuthority
            };

            cmdHandlerDelegates[cmdHash] = invoker;

            //string ingoreAuthorityMessage = invokerType == MirrorInvokeType.Command ? $" requiresAuthority:{cmdRequiresAuthority}" : "";
            //Debug.Log($"RegisterDelegate hash: {cmdHash} invokerType: {invokerType} method: {func.GetMethodName()}{ingoreAuthorityMessage}");

            return cmdHash;
        }

        private static bool CheckIfDeligateExists(Type invokeClass, MirrorInvokeType invokerType, CmdDelegate func,
            int cmdHash)
        {
            if (cmdHandlerDelegates.ContainsKey(cmdHash))
            {
                // something already registered this hash
                var oldInvoker = cmdHandlerDelegates[cmdHash];
                if (oldInvoker.AreEqual(invokeClass, invokerType, func))
                    // it's all right,  it was the same function
                    return true;

                Debug.LogError(
                    $"Function {oldInvoker.invokeClass}.{oldInvoker.invokeFunction.GetMethodName()} and {invokeClass}.{func.GetMethodName()} have the same hash.  Please rename one of them");
            }

            return false;
        }

        public static void RegisterCommandDelegate(Type invokeClass, string cmdName, CmdDelegate func,
            bool requiresAuthority)
        {
            RegisterDelegate(invokeClass, cmdName, MirrorInvokeType.Command, func, requiresAuthority);
        }

        public static void RegisterRpcDelegate(Type invokeClass, string rpcName, CmdDelegate func)
        {
            RegisterDelegate(invokeClass, rpcName, MirrorInvokeType.ClientRpc, func);
        }

        //  We need this in order to clean up tests
        internal static void RemoveDelegate(int hash)
        {
            cmdHandlerDelegates.Remove(hash);
        }

        private static bool GetInvokerForHash(int cmdHash, MirrorInvokeType invokeType, out Invoker invoker)
        {
            if (cmdHandlerDelegates.TryGetValue(cmdHash, out invoker) && invoker != null &&
                invoker.invokeType == invokeType) return true;

            // debug message if not found, or null, or mismatched type
            // (no need to throw an error, an attacker might just be trying to
            //  call an cmd with an rpc's hash)
            // Debug.Log("GetInvokerForHash hash:" + cmdHash + " not found");
            return false;
        }

        // InvokeCmd/Rpc Delegate can all use the same function here
        internal static bool InvokeHandlerDelegate(int cmdHash, MirrorInvokeType invokeType, NetworkReader reader,
            NetworkBehaviour invokingType, NetworkConnectionToClient senderConnection = null)
        {
            if (GetInvokerForHash(cmdHash, invokeType, out var invoker) &&
                invoker.invokeClass.IsInstanceOfType(invokingType))
            {
                invoker.invokeFunction(invokingType, reader, senderConnection);
                return true;
            }

            return false;
        }

        internal static CommandInfo GetCommandInfo(int cmdHash, NetworkBehaviour invokingType)
        {
            if (GetInvokerForHash(cmdHash, MirrorInvokeType.Command, out var invoker) &&
                invoker.invokeClass.IsInstanceOfType(invokingType))
                return new CommandInfo
                {
                    requiresAuthority = invoker.cmdRequiresAuthority
                };
            return default;
        }

        /// <summary>Gets the handler function by hash. Useful for profilers and debuggers.</summary>
        public static CmdDelegate GetDelegate(int cmdHash)
        {
            if (cmdHandlerDelegates.TryGetValue(cmdHash, out var invoker)) return invoker.invokeFunction;
            return null;
        }
    }
}