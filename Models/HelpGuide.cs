using System;

namespace RenPyTRLauncher.Models
{
    public enum HelpGuideType
    {
        Text = 0,
        Video = 1,
        FAQ = 2,
        Tool = 3
    }

    public class HelpGuide
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public HelpGuideType Type { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public string TypeIcon => Type switch
        {
            HelpGuideType.Text => "📄",
            HelpGuideType.Video => "🎬",
            HelpGuideType.FAQ => "❓",
            HelpGuideType.Tool => "🛠",
            _ => "📋"
        };

        public string TypeLabel => Type switch
        {
            HelpGuideType.Text => "Yazılı Rehber",
            HelpGuideType.Video => "Video Rehber",
            HelpGuideType.FAQ => "SSS",
            HelpGuideType.Tool => "Araç",
            _ => "Rehber"
        };
    }
}
