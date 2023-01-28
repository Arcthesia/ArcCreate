using System;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Attribute for whitelisting editor scopes for an action.
    /// </summary>
    public class WhitelistScopesAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhitelistScopesAttribute"/> class.
        /// </summary>
        /// <param name="scopes">Types of the scopes to whitelist.</param>
        public WhitelistScopesAttribute(params Type[] scopes)
        {
            Scopes = scopes;
        }

        public Type[] Scopes { get; private set; }
    }
}