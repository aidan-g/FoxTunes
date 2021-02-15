using FoxTunes.Mpeg4;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes.MetaData.TagLib.Tests
{
    [TestFixture]
    public class TagLibFileFactoryTests : TestBase
    {
        public override void SetUp()
        {
            ComponentResolver.Slots.Add(ComponentSlots.MetaData, "679D9459-BBCE-4D95-BB65-DD20C335719C");
            base.SetUp();
        }

        public override void TearDown()
        {
            ComponentResolver.Slots.Remove(ComponentSlots.MetaData);
            base.TearDown();
        }

        [TestCase(@"C:\Users\AidanG\Desktop\Test\1.m4a", "1")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\2.m4a", "2")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\3.m4a", "3")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\4.m4a", "4")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\5.m4a", "5")]
        public async Task CanReadXtraBox(string fileName, string rating)
        {
            //Turn on READ_WINDOWS_MEDIA_TAGS.
            var fileFactory = ComponentRegistry.Instance.GetComponent<TagLibFileFactory>();
            fileFactory.WindowsMedia.Value = true;
            var metaDataSource = this.Core.Factories.MetaDataSource.Create();
            var metaData = await metaDataSource.GetMetaData(fileName).ConfigureAwait(false);
            foreach (var metaDataItem in metaData)
            {
                if (string.Equals(metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual(rating, metaDataItem.Value);
                    return;
                }
            }
            Assert.Fail("No rating was returned.");
        }

        [TestCase(@"C:\Users\AidanG\Desktop\Test\6.m4a", "1")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\6.m4a", "2")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\6.m4a", "3")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\6.m4a", "4")]
        [TestCase(@"C:\Users\AidanG\Desktop\Test\6.m4a", "5")]
        public async Task CanReadAndWriteXtraBox(string fileName, string rating)
        {
            //Turn on READ_WINDOWS_MEDIA_TAGS.
            var fileFactory = ComponentRegistry.Instance.GetComponent<TagLibFileFactory>();
            fileFactory.WindowsMedia.Value = true;
            var metaDataSource = this.Core.Factories.MetaDataSource.Create();
            var metaData = new[]
            {
                new MetaDataItem(CommonStatistics.Rating, MetaDataItemType.Tag)
                {
                    Value = rating
                }
            };
            await metaDataSource.SetMetaData(fileName, metaData, null).ConfigureAwait(false);
            await this.CanReadXtraBox(fileName, rating).ConfigureAwait(false);
        }

        [Test]
        public void CanFormatXtraBox()
        {
            var tags = new[]
            {
                new XtraTag(
                    "WM/SharedUserRating",
                    new[]
                    {
                        new XtraTagPart(
                            XtraTagType.UInt64,
                            BitConverter.GetBytes(50ul)
                        )
                    }
                ),
                new XtraTag(
                    "WM/Category",
                    new[]
                    {
                        new XtraTagPart(
                            XtraTagType.Unicode,
                            Encoding.Unicode.GetBytes("Tag 1\0")
                        ),
                        new XtraTagPart(
                            XtraTagType.Unicode,
                            Encoding.Unicode.GetBytes("Tag 2\0")
                        ),
                        new XtraTagPart(
                            XtraTagType.Unicode,
                            Encoding.Unicode.GetBytes("Tag 3\0")
                        )
                    }
                )
            };
            var formatter = new XtraBoxFormatter(tags);
            var data = formatter.Data;
            var parser = new XtraBoxParser(data);
            Assert.IsTrue(Enumerable.SequenceEqual(
                tags,
                parser.Tags
            ));
        }
    }
}
