using FoxTunes.Interfaces;
using NUnit.Framework;

namespace FoxTunes
{
    [TestFixture]
    public class FilterParserTests : TestBase
    {
        [Test]
        public void FilterParserTest001()
        {
            var filter = "Hello World!";
            var expected = new FilterParserResult(
                new[]
                {
                    new FilterParserResultGroup(
                        new[]
                        {
                            new FilterParserResultEntry(CommonMetaData.Artist, FilterParserEntryOperator.Match, "*Hello World!*"),
                            new FilterParserResultEntry(CommonMetaData.Album, FilterParserEntryOperator.Match, "*Hello World!*"),
                            new FilterParserResultEntry(CommonMetaData.Title, FilterParserEntryOperator.Match, "*Hello World!*"),
                        },
                        FilterParserGroupOperator.Or
                    )
                }
            );
            var actual = default(IFilterParserResult);
            Assert.IsTrue(this.Core.Components.FilterParser.TryParse(
                filter,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilterParserTest002()
        {
            var filter = "artist:custard";
            var expected = new FilterParserResult(
                new[]
                {
                    new FilterParserResultGroup(
                        new[]
                        {
                            new FilterParserResultEntry(CommonMetaData.Artist, FilterParserEntryOperator.Match, "*custard*")
                        },
                        FilterParserGroupOperator.And
                    )
                }
            );
            var actual = default(IFilterParserResult);
            Assert.IsTrue(this.Core.Components.FilterParser.TryParse(
                filter,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilterParserTest003()
        {
            var filter = "artist:custard cream";
            var expected = new FilterParserResult(
                new[]
                {
                    new FilterParserResultGroup(
                        new[]
                        {
                            new FilterParserResultEntry(CommonMetaData.Artist, FilterParserEntryOperator.Match, "*custard cream*")
                        },
                        FilterParserGroupOperator.And
                    )
                }
            );
            var actual = default(IFilterParserResult);
            Assert.IsTrue(this.Core.Components.FilterParser.TryParse(
                filter,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilterParserTest004()
        {
            var filter = "artist:custard cream year>:1990 year<2000";
            var expected = new FilterParserResult(
                new[]
                {
                    new FilterParserResultGroup(
                        new[]
                        {
                            new FilterParserResultEntry(CommonMetaData.Artist, FilterParserEntryOperator.Match, "*custard cream*"),
                            new FilterParserResultEntry(CommonMetaData.Year, FilterParserEntryOperator.GreaterEqual, "1990"),
                            new FilterParserResultEntry(CommonMetaData.Year, FilterParserEntryOperator.Less, "2000"),
                        },
                        FilterParserGroupOperator.And
                    )
                }
            );
            var actual = default(IFilterParserResult);
            Assert.IsTrue(this.Core.Components.FilterParser.TryParse(
                filter,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FilterParserTest005()
        {
            var filter = "5* cream cakes";
            var expected = new FilterParserResult(
                new[]
                {
                    new FilterParserResultGroup(
                        new[]
                        {
                            new FilterParserResultEntry(CommonStatistics.Rating, FilterParserEntryOperator.Equal, "5"),
                        },
                        FilterParserGroupOperator.And
                    ),
                     new FilterParserResultGroup(
                        new[]
                        {
                            new FilterParserResultEntry(CommonMetaData.Artist, FilterParserEntryOperator.Match, "*cream cakes*"),
                            new FilterParserResultEntry(CommonMetaData.Album, FilterParserEntryOperator.Match, "*cream cakes*"),
                            new FilterParserResultEntry(CommonMetaData.Title, FilterParserEntryOperator.Match, "*cream cakes*"),
                        },
                        FilterParserGroupOperator.Or
                    )
                }
            );
            var actual = default(IFilterParserResult);
            Assert.IsTrue(this.Core.Components.FilterParser.TryParse(
                filter,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }
    }
}
