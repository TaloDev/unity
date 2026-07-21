using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class BuildObjectTest
    {
        [Test]
        public void BuildObject_NullFields_AreOmitted()
        {
            var json = JsonUtils.BuildObject(("name", null), ("score", 42));

            Assert.AreEqual("{\"score\":42}", json);
        }

        [Test]
        public void BuildObject_PropWithEmptyValue_SerializesAsNull()
        {
            var json = JsonUtils.BuildObject(("props", new Prop[] { new(("key1", "")) }));

            Assert.AreEqual("{\"props\":[{\"key\":\"key1\",\"value\":null}]}", json);
        }

        [Test]
        public void BuildObject_PropWithValue_SerializesAsString()
        {
            var json = JsonUtils.BuildObject(("props", new Prop[] { new(("key1", "hello")) }));

            Assert.AreEqual("{\"props\":[{\"key\":\"key1\",\"value\":\"hello\"}]}", json);
        }

        [Test]
        public void BuildObject_BoolValue_SerializesAsLiteral()
        {
            var json = JsonUtils.BuildObject(("enabled", true), ("disabled", false));

            Assert.AreEqual("{\"enabled\":true,\"disabled\":false}", json);
        }

        [Test]
        public void BuildObject_StringValue_SerializesAsQuotedString()
        {
            var json = JsonUtils.BuildObject(("name", "hello"));

            Assert.AreEqual("{\"name\":\"hello\"}", json);
        }

        [Test]
        public void BuildObject_IntValue_SerializesAsNumber()
        {
            var json = JsonUtils.BuildObject(("count", 42));

            Assert.AreEqual("{\"count\":42}", json);
        }

        [Test]
        public void BuildObject_LongValue_SerializesAsNumber()
        {
            var json = JsonUtils.BuildObject(("big", 9999999999L));

            Assert.AreEqual("{\"big\":9999999999}", json);
        }

        [Test]
        public void BuildObject_FloatValue_SerializesWithDotDecimal()
        {
            var json = JsonUtils.BuildObject(("rate", 1.5f));

            Assert.AreEqual("{\"rate\":1.5}", json);
        }

        [Test]
        public void BuildObject_DoubleValue_SerializesWithDotDecimal()
        {
            var json = JsonUtils.BuildObject(("rate", 2.5));

            Assert.AreEqual("{\"rate\":2.5}", json);
        }

        [Test]
        public void BuildObject_StringWithQuote_IsEscaped()
        {
            var json = JsonUtils.BuildObject(("name", "he said \"hi\""));

            Assert.AreEqual("{\"name\":\"he said \\\"hi\\\"\"}", json);
        }

        [Test]
        public void BuildObject_StringWithBackslash_IsEscaped()
        {
            var json = JsonUtils.BuildObject(("path", "a\\b"));

            Assert.AreEqual("{\"path\":\"a\\\\b\"}", json);
        }

        [Test]
        public void BuildObject_PropKeyWithQuote_IsEscaped()
        {
            var json = JsonUtils.BuildObject(("props", new Prop[] { new(("k\"y", "v")) }));

            Assert.AreEqual("{\"props\":[{\"key\":\"k\\\"y\",\"value\":\"v\"}]}", json);
        }

        [Test]
        public void BuildObject_PropValueWithBackslash_IsEscaped()
        {
            var json = JsonUtils.BuildObject(("props", new Prop[] { new(("k", "a\\b")) }));

            Assert.AreEqual("{\"props\":[{\"key\":\"k\",\"value\":\"a\\\\b\"}]}", json);
        }
    }
}
