using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class SanitiseJsonTest
    {
        [Test]
        public void SanitiseJson_EmptyValue_ConvertsToNull()
        {
            var result = Prop.SanitiseJson("{\"key\":\"key1\",\"value\":\"\"}");

            Assert.AreEqual("{\"key\":\"key1\",\"value\":null}", result);
        }

        [Test]
        public void SanitiseJson_NonEmptyValue_LeftUnchanged()
        {
            var result = Prop.SanitiseJson("{\"key\":\"key1\",\"value\":\"hello\"}");

            Assert.AreEqual("{\"key\":\"key1\",\"value\":\"hello\"}", result);
        }

        [Test]
        public void SanitiseJson_MultipleProps_OnlyEmptyValuesConverted()
        {
            var input = "{\"key\":\"a\",\"value\":\"\"},{\"key\":\"b\",\"value\":\"x\"}";

            var result = Prop.SanitiseJson(input);

            Assert.AreEqual("{\"key\":\"a\",\"value\":null},{\"key\":\"b\",\"value\":\"x\"}", result);
        }
    }
}
