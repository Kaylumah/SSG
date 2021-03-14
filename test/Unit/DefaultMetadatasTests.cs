using FluentAssertions;
using Kaylumah.Ssg.Manager.Site.Service;
using Xunit;

namespace Test.Unit
{
    public class DefaultMetadatasTests
    {
        [Fact]
        public void TestKey()
        {
            var itemWithoutScope = new DefaultMetadata() { Path = "" };
            var itemWithScope = new DefaultMetadata() { Path = "", Scope = "" };
            var itemWithNamedScope = new DefaultMetadata() { Path = "", Scope = "posts" };
            var itemPathWithNameScope = new DefaultMetadata() { Path = "2019", Scope = "posts" };

            var data = new DefaultMetadatas
            {
                itemWithoutScope,
                itemWithScope,
                itemWithNamedScope,
                itemPathWithNameScope
            };

            data[""].Should().NotBeNull();
            data["."].Should().NotBeNull();
            data[".posts"].Should().NotBeNull();
            data["2019.posts"].Should().NotBeNull();
        }
    }
}