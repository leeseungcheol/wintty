using System;
using Ghostty.Core.Ssh;
using Xunit;

namespace Ghostty.Tests.Ssh;

public class ClipboardImageUploadTests
{
    [Fact]
    public void FileName_Is_A_Timestamped_Png_Down_To_The_Millisecond()
    {
        var name = ClipboardImageUpload.FileName(new DateTime(2026, 11, 23, 9, 5, 7, 42));
        Assert.Equal("wintty-paste-20261123-090507-042.png", name);
    }

    [Theory]
    [InlineData("/home/me/work/shot.png", "/home/me/work/shot.png")]
    [InlineData("~/shot.png", "~/shot.png")]
    [InlineData("shot.png", "shot.png")]
    public void PromptText_Leaves_Plain_Paths_Alone(string path, string expected)
        => Assert.Equal(expected, ClipboardImageUpload.PromptText(path));

    [Fact]
    public void PromptText_Quotes_A_Path_With_A_Space()
        => Assert.Equal("'/home/me/my work/shot.png'", ClipboardImageUpload.PromptText("/home/me/my work/shot.png"));

    [Fact]
    public void PromptText_Keeps_The_Tilde_Outside_The_Quotes()
        => Assert.Equal("~/'my work/shot.png'", ClipboardImageUpload.PromptText("~/my work/shot.png"));

    [Fact]
    public void PromptText_Escapes_A_Single_Quote()
        => Assert.Equal("'/tmp/it'\\''s/shot.png'", ClipboardImageUpload.PromptText("/tmp/it's/shot.png"));

    [Theory]
    [InlineData("/tmp/$(rm -rf ~)/shot.png")]
    [InlineData("/tmp/a;b/shot.png")]
    [InlineData("/tmp/`x`/shot.png")]
    public void PromptText_Quotes_Shell_Syntax(string path)
    {
        var text = ClipboardImageUpload.PromptText(path);
        Assert.StartsWith("'", text);
        Assert.EndsWith("'", text);
    }
}
