namespace TheLightStore.Models.System;

public partial class SystemSetting
{
    public int Id { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string SettingType { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsPublic { get; set; }

    public string? Category { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
