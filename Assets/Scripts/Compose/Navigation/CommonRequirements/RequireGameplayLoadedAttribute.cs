namespace ArcCreate.Compose.Navigation
{
    public class RequireGameplayLoadedAttribute : ContextRequirementAttribute
    {
        public override bool CheckRequirement()
        {
            return Services.Gameplay?.IsLoaded ?? false;
        }
    }
}