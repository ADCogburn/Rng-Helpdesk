namespace RngHelpdesk.Contracts.Common;

/// <summary>
/// Represents a successful Command execution that has no response content.
/// </summary>
public sealed class Unit
{
    public static readonly Unit Value = new();
    private Unit() { }
}
