using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassStream : BaseComponent, IBassStream
    {
        public const int ENDING_THRESHOLD = 5;

        private BassStream()
        {
            this.Syncs = new int[] { };
            this.Errors = Errors.OK;
        }

        public BassStream(IBassStreamProvider provider, int channelHandle, long length, IEnumerable<IBassStreamAdvice> advice, BassFlags flags) : this()
        {
            this.Provider = provider;
            this.ChannelHandle = channelHandle;
            this.Length = length;
            this.Advice = advice;
            this.Flags = flags;
        }

        public IBassStreamProvider Provider { get; private set; }

        public int ChannelHandle { get; private set; }

        public int[] Syncs { get; private set; }

        public long Length { get; private set; }

        public virtual long Position
        {
            get
            {
                if (this.Provider == null)
                {
                    return 0;
                }
                return this.Provider.GetPosition(this.ChannelHandle);
            }
            set
            {
                this.Provider.SetPosition(this.ChannelHandle, value);
                this.OnPositionChanged();
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (this.Position > this.Length - Bass.ChannelSeconds2Bytes(this.ChannelHandle, ENDING_THRESHOLD))
            {
                this.OnEnding();
            }
        }

        public IEnumerable<IBassStreamAdvice> Advice { get; private set; }

        public BassFlags Flags { get; private set; }

        public Errors Errors { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return this.ChannelHandle == 0;
            }
        }

        public virtual void RegisterSyncHandlers()
        {
            this.Syncs = new int[]
            {
                 BassUtils.OK(Bass.ChannelSetSync(
                    this.ChannelHandle,
                    SyncFlags.Position,
                    this.Length - Bass.ChannelSeconds2Bytes(this.ChannelHandle, ENDING_THRESHOLD),
                    this.OnEnding
                )),
                BassUtils.OK(Bass.ChannelSetSync(
                    this.ChannelHandle,
                    SyncFlags.End,
                    0,
                    this.OnEnded
                ))
            };
        }

        protected virtual void OnEnding(int Handle, int Channel, int Data, IntPtr User)
        {
            //Critical: Don't block in this call back, it glitches playback.
            this.Dispatch(this.OnEnding);
        }

        protected virtual void OnEnding()
        {
            if (this.Ending != null)
            {
                this.Ending(this, EventArgs.Empty);
            }
        }

        public event EventHandler Ending;

        protected virtual void OnEnded(int Handle, int Channel, int Data, IntPtr User)
        {
            //Critical: Don't block in this call back, it glitches playback.
            this.Dispatch(this.OnEnded);
        }

        protected virtual void OnEnded()
        {
            if (this.Ended != null)
            {
                this.Ended(this, EventArgs.Empty);
            }
        }

        public event EventHandler Ended;

        public virtual bool CanReset
        {
            get
            {
                return true;
            }
        }

        public virtual void Reset()
        {
            this.Position = 0;
        }

        new public static IBassStream Error(Errors errors)
        {
            return new BassStream()
            {
                Errors = errors
            };
        }

        public static IBassStream Empty
        {
            get
            {
                return new BassStream();
            }
        }
    }
}
