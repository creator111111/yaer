// 在 Tests 文件夹里右键 → Create → C# Script
// 命名为 "ChestTests.cs"
// 粘贴下面代码：

using NUnit.Framework;

public class ChestTests
{
    [Test]
    public void 开箱子测试()
    {
        // 这是一个最简单的测试，只是验证环境
        Assert.IsTrue(true);
    }
}