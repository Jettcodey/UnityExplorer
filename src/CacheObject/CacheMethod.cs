using UnityExplorer.Inspectors;

namespace UnityExplorer.CacheObject
{
    public class CacheMethod : CacheMember
    {
        public MethodInfo MethodInfo { get; }
        public override Type DeclaringType => MethodInfo.DeclaringType;
        public override bool CanWrite => false;
        public override bool IsStatic => MethodInfo.IsStatic;

        public override bool ShouldAutoEvaluate => false;
#if CPP
        private bool? m_callsBrokenOutArray;
#endif

        public CacheMethod(MethodInfo mi)
        {
            this.MethodInfo = mi;
        }

        public override void SetInspectorOwner(ReflectionInspector inspector, MemberInfo member)
        {
            base.SetInspectorOwner(inspector, member);

            Arguments = MethodInfo.GetParameters();
            if (MethodInfo.IsGenericMethod)
                GenericArguments = MethodInfo.GetGenericArguments();
        }

        protected override object TryEvaluate()
        {
#if CPP
            // Checked on the first call only, the IL walk is too slow for every method of an inspected type
            m_callsBrokenOutArray ??= CacheProperty.CallsBrokenOutArray(MethodInfo);
            if (m_callsBrokenOutArray.Value)
            {
                LastException = new NotSupportedException(CacheProperty.BrokenOutArrayMessage);
                return null;
            }
#endif
            try
            {
                MethodInfo methodInfo = MethodInfo;
                if (methodInfo.IsGenericMethod)
                    methodInfo = MethodInfo.MakeGenericMethod(Evaluator.TryParseGenericArguments());

                object ret;
                if (HasArguments)
                    ret = methodInfo.Invoke(DeclaringInstance, Evaluator.TryParseArguments());
                else
                    ret = methodInfo.Invoke(DeclaringInstance, ArgumentUtility.EmptyArgs);
                LastException = null;
                return ret;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return null;
            }
        }

        protected override void TrySetValue(object value) => throw new NotImplementedException("You can't set a method");
    }
}
