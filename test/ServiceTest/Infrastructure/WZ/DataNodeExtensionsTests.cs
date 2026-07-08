using Application.Templates.Reader.Img;
using Duey.Abstractions;
using Moq;

namespace ServiceTest.Infrastructure.WZ;

internal class DataNodeExtensionsTests
{
    static IDataProperty<T> AsProperty<T>(T value, string name = "node")
    {
        var mock = new Mock<IDataProperty<T>>();
        mock.Setup(x => x.Name).Returns(name);
        mock.Setup(x => x.Resolve()).Returns(value);
        mock.Setup(x => x.Children).Returns(Enumerable.Empty<IDataNode>());
        return mock.Object;
    }

    static IDataProperty<T> AsProperty<T>(T value, string name, IDataNode parent)
    {
        var mock = new Mock<IDataProperty<T>>();
        mock.Setup(x => x.Name).Returns(name);
        mock.Setup(x => x.Resolve()).Returns(value);
        mock.Setup(x => x.Children).Returns(Enumerable.Empty<IDataNode>());
        mock.Setup(x => x.Parent).Returns(parent);
        return mock.Object;
    }

    [Test]
    public void GetIntValue_ReturnsPropertyLong_WhenNodeIsIntProperty()
    {
        var node = AsProperty<long>(42);
        var result = node.GetIntValue();
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void GetIntValue_ReturnsPropertyLong_WhenNodeIsDoubleProperty()
    {
        var node = AsProperty<double>(3.14);
        var result = node.GetIntValue();
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void GetIntValue_ParsesString_WhenNodeIsStringProperty()
    {
        var node = AsProperty<string>("7835");
        var result = node.GetIntValue();
        Assert.That(result, Is.EqualTo(7835));
    }

    [Test]
    public void GetIntValue_ReturnsDefault_WhenNodeIsStringPropertyWithInvalidValue()
    {
        var node = AsProperty<string>("not-a-number");
        var result = node.GetIntValue(defaultValue: -1);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetIntValue_ReturnsDefault_WhenPathNotFound()
    {
        var nodeMock = new Mock<IDataNode>();
        nodeMock.Setup(x => x.Name).Returns("root");
        nodeMock.Setup(x => x.Children).Returns(Enumerable.Empty<IDataNode>());

        var result = nodeMock.Object.GetIntValue("nonexistent/path", 99);
        Assert.That(result, Is.EqualTo(99));
    }

    [Test]
    public void GetLongValue_ReturnsPropertyLong()
    {
        var node = AsProperty<long>(long.MaxValue);
        var result = node.GetLongValue();
        Assert.That(result, Is.EqualTo(long.MaxValue));
    }

    [Test]
    public void GetLongValue_ReturnsDefault_WhenNoMatch()
    {
        var nodeMock = new Mock<IDataNode>();
        nodeMock.Setup(x => x.Name).Returns("empty");
        nodeMock.Setup(x => x.Children).Returns(Enumerable.Empty<IDataNode>());

        var result = nodeMock.Object.GetLongValue(defaultValue: -1);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetShortValue_ReturnsCorrectValue()
    {
        var node = AsProperty<long>(255);
        var result = node.GetShortValue();
        Assert.That(result, Is.EqualTo((short)255));
    }

    [Test]
    public void GetByteValue_ReturnsCorrectValue()
    {
        var node = AsProperty<long>(200);
        var result = node.GetByteValue();
        Assert.That(result, Is.EqualTo((byte)200));
    }

    [Test]
    public void GetDoubleValue_ReturnsDouble_WhenNodeIsDoubleProperty()
    {
        var node = AsProperty<double>(3.14159);
        var result = node.GetDoubleValue();
        Assert.That(result, Is.EqualTo(3.14159));
    }

    [Test]
    public void GetDoubleValue_ReturnsDouble_WhenNodeIsLongProperty()
    {
        var node = AsProperty<long>(42);
        var result = node.GetDoubleValue();
        Assert.That(result, Is.EqualTo(42.0));
    }

    [Test]
    public void GetDoubleValue_ParsesString_WhenNodeIsStringProperty()
    {
        var node = AsProperty<string>("3.14");
        var result = node.GetDoubleValue();
        Assert.That(result, Is.EqualTo(3.14));
    }

    [Test]
    public void GetDoubleValue_HandlesEuropeanFormat()
    {
        var node = AsProperty<string>("3,14");
        var result = node.GetDoubleValue();
        Assert.That(result, Is.EqualTo(3.14));
    }

    [Test]
    public void GetFloatValue_ReturnsCorrectValue()
    {
        var node = AsProperty<double>(2.5f);
        var result = node.GetFloatValue();
        Assert.That(result, Is.EqualTo(2.5f));
    }

    [Test]
    public void GetStringValue_ReturnsString_WhenNodeIsStringProperty()
    {
        var node = AsProperty<string>("hello");
        var result = node.GetStringValue();
        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public void GetStringValue_ReturnsString_WhenNodeIsLongProperty()
    {
        var node = AsProperty<long>(42);
        var result = node.GetStringValue();
        Assert.That(result, Is.EqualTo("42"));
    }

    [Test]
    public void GetStringValue_ReturnsString_WhenNodeIsDoubleProperty()
    {
        var node = AsProperty<double>(3.14);
        var result = node.GetStringValue();
        Assert.That(result, Is.EqualTo("3.14"));
    }

    [Test]
    public void GetStringValue_ReturnsNull_WhenNoMatch()
    {
        var nodeMock = new Mock<IDataNode>();
        nodeMock.Setup(x => x.Name).Returns("empty");
        nodeMock.Setup(x => x.Children).Returns(Enumerable.Empty<IDataNode>());

        var result = nodeMock.Object.GetStringValue();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetStringValue_ReturnsNull_WhenPathNotFound()
    {
        var nodeMock = new Mock<IDataNode>();
        nodeMock.Setup(x => x.Name).Returns("root");
        nodeMock.Setup(x => x.Children).Returns(Enumerable.Empty<IDataNode>());

        var result = nodeMock.Object.GetStringValue("missing/path");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetIntValue_WithPath_ResolvesAndConverts()
    {
        var child = AsProperty<long>(100, "child");
        var rootMock = new Mock<IDataNode>();
        rootMock.Setup(x => x.Name).Returns("root");
        rootMock.Setup(x => x.Children).Returns([child]);

        var result = rootMock.Object.GetIntValue("child");
        Assert.That(result, Is.EqualTo(100));
    }

    [Test]
    public void GetStringValue_WithPath_ResolvesAndReturns()
    {
        var child = AsProperty<string>("nested-value", "child");
        var rootMock = new Mock<IDataNode>();
        rootMock.Setup(x => x.Name).Returns("root");
        rootMock.Setup(x => x.Children).Returns([child]);

        var result = rootMock.Object.GetStringValue("child");
        Assert.That(result, Is.EqualTo("nested-value"));
    }

    [Test]
    public void GetDoubleValue_WithPath_ResolvesAndConverts()
    {
        var child = AsProperty<double>(1.5, "rate");
        var rootMock = new Mock<IDataNode>();
        rootMock.Setup(x => x.Name).Returns("root");
        rootMock.Setup(x => x.Children).Returns([child]);

        var result = rootMock.Object.GetDoubleValue("rate");
        Assert.That(result, Is.EqualTo(1.5));
    }
}
