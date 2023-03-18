namespace Content.Server.Speech.Components
{
    [RegisterComponent]
    public sealed class PrisonerAccentComponent : Component
    {
        [DataField("Jailer"), ViewVariables(VVAccess.ReadWrite)]
        public bool Jailer = true;
    }
}
