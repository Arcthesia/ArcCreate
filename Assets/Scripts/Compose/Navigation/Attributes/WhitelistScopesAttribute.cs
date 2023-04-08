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
            All = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhitelistScopesAttribute"/> class.
        /// </summary>
        /// <param name="all">Whether or not to whitelist all scopes.</param>
        public WhitelistScopesAttribute(bool all)
        {
            Scopes = new Type[0];
            All = all;
        }

        public Type[] Scopes { get; private set; }

        public bool All { get; private set; }
    }
}