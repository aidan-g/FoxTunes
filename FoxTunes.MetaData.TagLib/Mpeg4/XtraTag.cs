using System;
using System.Linq;

namespace FoxTunes.Mpeg4
{
    public class XtraTag : IEquatable<XtraTag>
    {
        public const string SharedUserRating = "WM/SharedUserRating";

        public XtraTag(string name, XtraTagPart[] parts)
        {
            this.Name = name;
            this.Parts = parts;
        }

        public string Name { get; private set; }

        public XtraTagPart[] Parts { get; private set; }

        public virtual bool Equals(XtraTag other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!Enumerable.SequenceEqual(this.Parts, other.Parts))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as XtraTag);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    hashCode += this.Name.ToLower().GetHashCode();
                }
                if (this.Parts != null)
                {
                    foreach (var part in this.Parts)
                    {
                        hashCode += part.GetHashCode();
                    }
                }
            }
            return hashCode;
        }

        public static bool operator ==(XtraTag a, XtraTag b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(XtraTag a, XtraTag b)
        {
            return !(a == b);
        }

        public static bool CanImport(XtraTag tag)
        {
            if (string.Equals(tag.Name, SharedUserRating, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public static bool CanExport(MetaDataItem metaDataItem)
        {
            if (string.Equals(metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public MetaDataItem ToMetaDataItem()
        {
            if (string.Equals(this.Name, SharedUserRating, StringComparison.OrdinalIgnoreCase))
            {
                return new MetaDataItem(CommonStatistics.Rating, MetaDataItemType.Tag)
                {
                    Value = Convert.ToString(GetRatingStars(BitConverter.ToUInt64(this.Parts[0].Content, 0)))
                };
            }
            throw new NotImplementedException();
        }

        public static XtraTag FromMetaDataItem(MetaDataItem metaDataItem)
        {
            if (string.Equals(metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase))
            {
                return new XtraTag(
                    SharedUserRating,
                    new[]
                    {
                        new XtraTagPart(
                            XtraTagType.UInt64,
                            BitConverter.GetBytes(GetRatingMask(metaDataItem.Value))
                        )
                    }
                );
            }
            throw new NotImplementedException();
        }

        private static ulong GetRatingMask(string rating)
        {
            var temp = default(byte);
            if (!byte.TryParse(rating, out temp))
            {
                return 0;
            }
            return GetRatingMask(temp);
        }

        private static ulong GetRatingMask(byte rating)
        {
            const byte RATING_0 = 0;
            const byte RATING_1 = 1;
            const byte RATING_2 = 25;
            const byte RATING_3 = 50;
            const byte RATING_4 = 75;
            const byte RATING_5 = 99;
            switch (rating)
            {
                default:
                    return RATING_0;
                case 1:
                    return RATING_1;
                case 2:
                    return RATING_2;
                case 3:
                    return RATING_3;
                case 4:
                    return RATING_4;
                case 5:
                    return RATING_5;
            }
        }

        private static byte GetRatingStars(ulong rating)
        {
            const byte RATING_0 = 0;
            const byte RATING_1 = 1;
            const byte RATING_2 = 2;
            const byte RATING_3 = 3;
            const byte RATING_4 = 4;
            const byte RATING_5 = 5;
            switch (rating)
            {
                default:
                    return RATING_0;
                case 1:
                    return RATING_1;
                case 25:
                    return RATING_2;
                case 50:
                    return RATING_3;
                case 75:
                    return RATING_4;
                case 99:
                    return RATING_5;
            }
        }
    }

    public enum XtraTagType : byte
    {
        None,
        Unicode,
        UInt64,
        Date,
        Guid,
        Variant,
        Unknown
    }
}
