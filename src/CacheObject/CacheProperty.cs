using UnityExplorer.Inspectors;

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
            else
            {
                m_shouldAutoEvaluate = true;
            }
        }

        public override void SetInspectorOwner(ReflectionInspector inspector, MemberInfo member)
        {
            base.SetInspectorOwner(inspector, member);

            Arguments = PropertyInfo.GetIndexParameters();
        }

        protected override object TryEvaluate()
        {
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
