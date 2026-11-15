using UnityExplorer.Inspectors;
#if CPP
using System.Reflection.Emit;
#endif

namespace UnityExplorer.CacheObject
{
    public class CacheProperty : CacheMember
    {
        public PropertyInfo PropertyInfo { get; internal set; }
        public override Type DeclaringType => PropertyInfo.DeclaringType;
        public override bool CanWrite => PropertyInfo.CanWrite;
        public override bool IsStatic => m_isStatic ?? (bool)(m_isStatic = PropertyInfo.GetAccessors(true)[0].IsStatic);
        private bool? m_isStatic;
        private bool m_shouldAutoEvaluate;
        private string m_skipReason;
#if CPP
        private bool m_callsBrokenOutArray;
#endif
        protected override string NOT_YET_EVAL => m_skipReason ?? base.NOT_YET_EVAL;
        // Not "perfect" yet, needs some more work
        public override bool ShouldAutoEvaluate
        {
            get
            {
                if (HasArguments) 
                    return false;

                if (LastException != null)
                    return false;

                return m_shouldAutoEvaluate;
            }
        }

        public CacheProperty(PropertyInfo pi)
        {
            this.PropertyInfo = pi;

            bool isPointer = PropertyInfo.PropertyType.IsPointer;
            bool isObsolete = PropertyInfo.IsDefined(typeof(ObsoleteAttribute), true);

            if (isObsolete)
            {
                m_skipReason = "<color=yellow>Skipped [Obsolete] attribute</color>";
                m_shouldAutoEvaluate = false;
            }
            else if (isPointer)
            {
                m_skipReason = "<color=red>Skipped an Unmanaged Pointer</color>";
                m_shouldAutoEvaluate = false;
            }
#if CPP
            else if (GetRestoredGetterSkipReason(pi, out m_callsBrokenOutArray) is string skipReason)
            {
                m_skipReason = skipReason;
                m_shouldAutoEvaluate = false;
            }
#endif
            else
            {
                m_shouldAutoEvaluate = true;
            }
        }

#if CPP
        private static readonly Dictionary<Type, bool> interopTypes = new();

        // Restored getters (Unity IL copied by Il2CppInterop) can crash the game, the checks below find the patterns that do
        internal const string BrokenOutArrayMessage = "Not called, it would crash the game: this method or one it calls has an [Out] array parameter, "
            + "and Il2CppInterop older than 1.5.3 (MelonLoader, BepInEx 6 before be.777) gives the game null instead of the array";

        private static string GetRestoredGetterSkipReason(PropertyInfo pi, out bool callsBrokenOutArray)
        {
            callsBrokenOutArray = false;
            MethodInfo getter = pi.GetGetMethod(true);
            if (getter == null || ExistsInGame(getter) || !IsInteropType(getter.DeclaringType))
                return null;

            bool isCollection = ReflectionUtility.TryGetEntryType(pi.PropertyType, out Type entryType);
            Type type = isCollection ? entryType : pi.PropertyType;
            // A stripped enum is only a number, every other stripped type (like Collider.GeometryHolder's struct) can crash
            if ((isCollection || !type.IsEnum) && !Il2CppReflection.Il2CppTypeNotNull(type))
                return "<color=yellow>Skipped, returns a type stripped from this game</color>";

            try
            {
                if (HasBrokenRestoredIL(getter, 0, true))
                {
                    callsBrokenOutArray = true;
                    return "<color=yellow>Skipped, calls a method with an [Out] array parameter this Il2CppInterop version breaks</color>";
                }

                if (HasBrokenRestoredIL(getter, 0, false))
                    return "<color=yellow>Skipped, restored by Il2CppInterop with broken IL</color>";
            }
            catch (TypeLoadException)
            {
                // Some interop signatures can't be loaded, the getter itself still works
            }

            return null;
        }

        // Il2CppInterop puts [CachedScanResults] on every method that exists in the game, IsDefined is faster than CustomAttributes for the console scan
        private static readonly Type cachedScanResultsAttribute = ReflectionUtility.GetTypeByName("Il2CppInterop.Common.Attributes.CachedScanResultsAttribute");

        private static bool ExistsInGame(MethodBase method)
            => cachedScanResultsAttribute != null
                ? method.IsDefined(cachedScanResultsAttribute, false)
                : method.CustomAttributes.Any(a => a.AttributeType.Name == "CachedScanResultsAttribute");

        private static bool IsInteropType(Type type)
        {
            if (!interopTypes.TryGetValue(type, out bool isInteropType))
            {
                isInteropType = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Any(ExistsInGame);
                interopTypes.Add(type, isInteropType);
            }
            return isInteropType;
        }

        private static readonly Dictionary<short, OpCode> opCodes = typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => (OpCode)f.GetValue(null))
            .ToDictionary(o => o.Value);

        // Methods UE or the C# console call directly: a generated method with a broken [Out] array parameter, or a restored one calling one
        internal static bool CallsBrokenOutArray(MethodBase method, bool checkRestored = true)
        {
            try
            {
                if (HasBrokenOutArrayParam(method))
                    return true;

                return checkRestored && !ExistsInGame(method) && method.DeclaringType != null && IsInteropType(method.DeclaringType)
                    && HasBrokenRestoredIL(method, 0, true);
            }
            catch (TypeLoadException)
            {
                return false;
            }
        }

        // Walks the restored IL, and the restored methods it calls, for the broken IL patterns described above
        private static bool HasBrokenRestoredIL(MethodBase method, int depth, bool onlyOutArrays)
        {
            byte[] il = method.GetMethodBody()?.GetILAsByteArray();
            if (il == null)
                return false;

            bool pushedManagedValue = false;
            for (int i = 0; i < il.Length;)
            {
                OpCode op = opCodes[il[i] == 0xFE ? (short)(0xFE00 | il[i + 1]) : il[i]];
                i += op.Size;

                // Only a string or boxed value pushed right before the call ends up in its last argument
                bool lastArgIsManagedValue = pushedManagedValue;
                pushedManagedValue = op == OpCodes.Ldstr || op == OpCodes.Box;

                if ((op == OpCodes.Call || op == OpCodes.Callvirt || op == OpCodes.Newobj)
                    && ResolveMethod(method, BitConverter.ToInt32(il, i)) is MethodBase target)
                {
                    if (!onlyOutArrays && lastArgIsManagedValue && LastArgIsIl2CppReference(target))
                        return true;

                    if (ExistsInGame(target))
                    {
                        if ((!onlyOutArrays && UsesValueTypeGenerics(target)) || HasBrokenOutArrayParam(target))
                            return true;
                    }
                    else if (depth < 3 && target.DeclaringType != null && IsInteropType(target.DeclaringType) && HasBrokenRestoredIL(target, depth + 1, onlyOutArrays))
                        return true;
                }

                i += OperandSize(op, il, i);
            }
            return false;
        }

        // The generated method writes the [Out] array back with stind.ref, 1.5.3 passes the array itself and never does
        private static bool HasBrokenOutArrayParam(MethodBase method)
        {
            if (!method.GetParameters().Any(p => p.IsOut && !p.ParameterType.IsByRef && !p.ParameterType.IsValueType))
                return false;

            byte[] il = method.GetMethodBody()?.GetILAsByteArray();
            if (il == null)
                return false;

            for (int i = 0; i < il.Length;)
            {
                OpCode op = opCodes[il[i] == 0xFE ? (short)(0xFE00 | il[i + 1]) : il[i]];
                i += op.Size;

                if (op == OpCodes.Stind_Ref)
                    return true;

                i += OperandSize(op, il, i);
            }
            return false;
        }

        private static int OperandSize(OpCode op, byte[] il, int i) => op.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + BitConverter.ToInt32(il, i) * 4,
            _ => 4,
        };

        private static MethodBase ResolveMethod(MethodBase caller, int methodToken)
        {
            try
            {
                return caller.Module.ResolveMethod(methodToken,
                    caller.DeclaringType.IsGenericType ? caller.DeclaringType.GetGenericArguments() : null,
                    caller.IsGenericMethod ? caller.GetGenericArguments() : null);
            }
            catch
            {
                // Restored IL can reference members the game doesn't have, calling those throws instead of crashing
                return null;
            }
        }

        private static bool LastArgIsIl2CppReference(MethodBase method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length > 0
                ? typeof(Il2CppSystem.Object).IsAssignableFrom(parameters[parameters.Length - 1].ParameterType)
                : !method.IsStatic && typeof(Il2CppSystem.Object).IsAssignableFrom(method.DeclaringType);
        }

        private static bool UsesValueTypeGenerics(MethodBase method)
            => (method.IsGenericMethod && method.GetGenericArguments().Any(t => t.IsValueType))
            || (method.DeclaringType.IsGenericType && method.DeclaringType.GetGenericArguments().Any(t => t.IsValueType));
#endif

        public override void SetInspectorOwner(ReflectionInspector inspector, MemberInfo member)
        {
            base.SetInspectorOwner(inspector, member);

            Arguments = PropertyInfo.GetIndexParameters();
        }

        protected override object TryEvaluate()
        {
#if CPP
            if (m_callsBrokenOutArray)
            {
                LastException = new NotSupportedException(BrokenOutArrayMessage);
                return null;
            }
#endif
            try
            {
                object ret;
                if (HasArguments)
                    ret = PropertyInfo.GetValue(DeclaringInstance, this.Evaluator.TryParseArguments());
                else
                    ret = PropertyInfo.GetValue(DeclaringInstance, null);
                LastException = null;
                return ret;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return null;
            }
        }

        protected override void TrySetValue(object value)
        {
            if (!CanWrite)
                return;

            try
            {
                bool _static = PropertyInfo.GetAccessors(true)[0].IsStatic;

                if (HasArguments)
                    PropertyInfo.SetValue(DeclaringInstance, value, Evaluator.TryParseArguments());
                else
                    PropertyInfo.SetValue(DeclaringInstance, value, null);
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning(ex);
            }
        }
    }
}
