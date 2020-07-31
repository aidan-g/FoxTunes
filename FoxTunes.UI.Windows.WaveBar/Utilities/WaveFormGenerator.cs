using FoxTunes.Interfaces;
using System;
using System.Windows.Controls;

namespace FoxTunes
{
    public static class WaveFormGenerator
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly IntegerConfigurationElement Resolution = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<IntegerConfigurationElement>(
            WaveBarBehaviourConfiguration.SECTION,
            WaveBarBehaviourConfiguration.RESOLUTION_ELEMENT
        );

        public static WaveFormGeneratorData Create(IOutputStream stream)
        {
            var length = Convert.ToInt32(Math.Ceiling(
                stream.GetDuration(stream.Length).TotalMilliseconds / Resolution.Value
            ));
            return new WaveFormGeneratorData()
            {
                Resolution = Resolution.Value,
                Data = new WaveFormDataElement[length, stream.Channels],
                Position = 0,
                Capacity = length,
                Peak = 0,
                Channels = stream.Channels,
                CancellationToken = new CancellationToken(),
            };
        }

        public static void Populate(IOutputStream stream, WaveFormGeneratorData data)
        {
            switch (stream.Format)
            {
                case OutputStreamFormat.Short:
                    PopulateShort(stream, data);
                    break;
                case OutputStreamFormat.Float:
                    PopulateFloat(stream, data);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (data.CancellationToken.IsCancellationRequested)
            {
                Logger.Write(typeof(WaveFormGenerator), LogLevel.Debug, "Wave form generation for file \"{0}\" was cancelled.", stream.FileName);
                return;
            }

            Logger.Write(typeof(WaveFormGenerator), LogLevel.Debug, "Wave form generated for file \"{0}\" with {1} elements: Peak = {2:0.00}", stream.FileName, data.Capacity, data.Peak);

            try
            {
                WaveFormCache.Save(stream, data);
            }
            catch (Exception e)
            {
                Logger.Write(typeof(WaveFormGenerator), LogLevel.Warn, "Failed to save wave form data for file \"{0}\": {1}", stream.FileName, e.Message);
            }
        }

        private static void PopulateShort(IOutputStream stream, WaveFormGeneratorData data)
        {
            var duration = TimeSpan.FromMilliseconds(data.Resolution);
            var buffer = stream.GetBuffer<short>(duration);
            var interval = data.Capacity / 10;

            Logger.Write(typeof(WaveFormGenerator), LogLevel.Debug, "Creating 16 bit wave form for file \"{0}\" with resolution of {1}ms", stream.FileName, duration.TotalMilliseconds);

            do
            {
#if DEBUG
                if (data.Position >= data.Capacity)
                {
                    //TODO: Why?
                    break;
                }
#endif

                var length = stream.GetData(buffer) / sizeof(short);
                if (length <= 0)
                {
                    break;
                }

                for (var a = 0; a < length; a += data.Channels)
                {
                    for (var b = 0; b < data.Channels; b++)
                    {
                        var value = (float)buffer[a + b] / short.MaxValue;
                        data.Data[data.Position, b].Min = Math.Min(data.Data[data.Position, b].Min, value);
                        data.Data[data.Position, b].Max = Math.Max(data.Data[data.Position, b].Max, value);
                        data.Data[data.Position, b].Rms += value * value;
                    }
                }

                for (var a = 0; a < data.Channels; a++)
                {
                    data.Data[data.Position, a].Rms = Convert.ToSingle(
                        Math.Sqrt(data.Data[data.Position, a].Rms / (length / data.Channels))
                    );

                    data.Peak = Math.Max(
                        data.Peak,
                        Math.Max(
                            Math.Abs(data.Data[data.Position, a].Min),
                            Math.Abs(data.Data[data.Position, a].Max)
                        )
                    );
                }

                data.Position++;

                if (data.Position % interval == 0)
                {
                    data.Update();
                }

            } while (!data.CancellationToken.IsCancellationRequested);

            data.Update();
        }

        private static void PopulateFloat(IOutputStream stream, WaveFormGeneratorData data)
        {
            var duration = TimeSpan.FromMilliseconds(data.Resolution);
            var buffer = stream.GetBuffer<float>(duration);
            var interval = data.Capacity / 10;

            Logger.Write(typeof(WaveFormGenerator), LogLevel.Debug, "Creating 32 bit wave form for file \"{0}\" with resolution of {1}ms", stream.FileName, duration.TotalMilliseconds);

            do
            {
#if DEBUG
                if (data.Position >= data.Capacity)
                {
                    //TODO: Why?
                    break;
                }
#endif

                var length = stream.GetData(buffer) / sizeof(float);
                if (length <= 0)
                {
                    break;
                }

                for (var a = 0; a < length; a += data.Channels)
                {
                    for (var b = 0; b < data.Channels; b++)
                    {
                        var value = buffer[a + b];
                        data.Data[data.Position, b].Min = Math.Min(data.Data[data.Position, b].Min, value);
                        data.Data[data.Position, b].Max = Math.Max(data.Data[data.Position, b].Max, value);
                        data.Data[data.Position, b].Rms += value * value;
                    }
                }

                for (var a = 0; a < data.Channels; a++)
                {
                    data.Data[data.Position, a].Rms = Convert.ToSingle(
                        Math.Sqrt(data.Data[data.Position, a].Rms / (length / data.Channels))
                    );

                    data.Peak = Math.Max(
                        data.Peak,
                        Math.Max(
                            Math.Abs(data.Data[data.Position, a].Min),
                            Math.Abs(data.Data[data.Position, a].Max)
                        )
                    );
                }

                data.Position++;

                if (data.Position % interval == 0)
                {
                    data.Update();
                }

            } while (!data.CancellationToken.IsCancellationRequested);

            data.Update();
        }

        [Serializable]
        public class WaveFormGeneratorData
        {
            public int Resolution;

            public WaveFormDataElement[,] Data;

            public int Channels;

            public int Position;

            public int Capacity;

            public float Peak;

            public void Update()
            {
                if (this.Updated == null)
                {
                    return;
                }
                this.Updated(this, EventArgs.Empty);
            }

            [field: NonSerialized]
            public event EventHandler Updated;

            [field: NonSerialized]
            public CancellationToken CancellationToken;
        }

        [Serializable]
        public struct WaveFormDataElement
        {
            public float Min;

            public float Max;

            public float Rms;
        }
    }
}
