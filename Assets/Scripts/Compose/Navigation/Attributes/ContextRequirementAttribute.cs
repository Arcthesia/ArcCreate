using System;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Base attribute for marking an editor action with a context requirement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class ContextRequirementAttribute : Attribute, IContextRequirement
    {
        public abstract bool CheckRequirement();
    }
}