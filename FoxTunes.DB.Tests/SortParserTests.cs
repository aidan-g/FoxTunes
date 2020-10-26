using FoxTunes.Interfaces;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace FoxTunes
{
    [TestFixture]
    public class SortParserTests : TestBase
    {
        [Test]
        public void SortParserTest001()
        {
            var sort = "CustomMetaData.VariousArtists";
            var expected = new SortParserResult(
                new[]
                {
                    new SortParserResultExpression(
                        CustomMetaData.VariousArtists
                    )
                }
            );
            var actual = default(ISortParserResult);
            Assert.IsTrue(this.Core.Components.SortParser.TryParse(
                sort,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SortParserTest002()
        {
            var sort = "CustomMetaData.VariousArtists ? CommonMetaData.Artist";
            var expected = new SortParserResult(
                new[]
                {
                    new SortParserResultExpression(
                        CustomMetaData.VariousArtists,
                        SortParserResultOperator.NullCoalesce,
                        new SortParserResultExpression(
                            CommonMetaData.Artist
                        )
                    )
                }
            );
            var actual = default(ISortParserResult);
            Assert.IsTrue(this.Core.Components.SortParser.TryParse(
                sort,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SortParserTest003()
        {
            var sort = "CommonMetaData.Disc num";
            var expected = new SortParserResult(
                new[]
                {
                    new SortParserResultExpression(
                        CommonMetaData.Disc,
                        SortParserResultOperator.Numeric
                    )
                }
            );
            var actual = default(ISortParserResult);
            Assert.IsTrue(this.Core.Components.SortParser.TryParse(
                sort,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SortParserTest004()
        {
            var sort = @"CustomMetaData.VariousArtists ? CommonMetaData.Artist
CommonMetaData.Year
CommonMetaData.Album
CommonMetaData.Disc num
CommonMetaData.Track num";
            var expected = new SortParserResult(
                new[]
                {
                    new SortParserResultExpression(
                        CustomMetaData.VariousArtists,
                        SortParserResultOperator.NullCoalesce,
                        new SortParserResultExpression(
                            CommonMetaData.Artist
                        )
                    ),
                    new SortParserResultExpression(
                        CommonMetaData.Year
                    ),
                    new SortParserResultExpression(
                        CommonMetaData.Album
                    ),
                    new SortParserResultExpression(
                        CommonMetaData.Disc,
                        SortParserResultOperator.Numeric
                    ),
                    new SortParserResultExpression(
                        CommonMetaData.Track,
                        SortParserResultOperator.Numeric
                    )
                }
            );
            var actual = default(ISortParserResult);
            Assert.IsTrue(this.Core.Components.SortParser.TryParse(
                sort,
                out actual
            ));
            Assert.AreEqual(expected, actual);
        }
    }
}
