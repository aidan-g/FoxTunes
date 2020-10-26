using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassEncoder : BaseComponent, IBassEncoder
    {
        static BassEncoder()
        {
            BassPluginLoader.Instance.Load();
        }

        private BassEncoder()
        {
            this.CancellationToken = new CancellationToken();
        }

        public BassEncoder(IEnumerable<EncoderItem> encoderItems) : this()
        {
            this.EncoderItems = encoderItems;
        }

        public CancellationToken CancellationToken { get; private set; }

        public Process Process
        {
            get
            {
                return Process.GetCurrentProcess();
            }
        }

        public IEnumerable<EncoderItem> EncoderItems { get; private set; }

        public void Encode()
        {
            Logger.Write(this, LogLevel.Debug, "Initializing BASS (NoSound).");
            Bass.Init(Bass.NoSoundDevice);
            try
            {
                var threads = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<IntegerConfigurationElement>(
                    BassEncoderBehaviourConfiguration.SECTION,
                    BassEncoderBehaviourConfiguration.THREADS_ELEMENT
                );
                this.Encode(threads.Value);
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "Releasing BASS (NoSound).");
                Bass.Free();
            }
        }

        protected virtual void Encode(int threads)
        {
            if (threads > 1)
            {
                Logger.Write(this, LogLevel.Debug, "Beginning parallel encoding with {0} threads.", threads);
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Beginning single threaded encoding.");
            }
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = threads
            };
            Parallel.ForEach(this.EncoderItems, parallelOptions, encoderItem =>
            {
                try
                {
                    var settings = ComponentRegistry.Instance.GetComponent<BassEncoderSettingsFactory>().CreateSettings(encoderItem.Profile);
                    if (this.CancellationToken.IsCancellationRequested)
                    {
                        Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", encoderItem.InputFileName);
                        encoderItem.Status = EncoderItemStatus.Cancelled;
                        return;
                    }
                    if (!this.CheckInput(encoderItem.InputFileName))
                    {
                        Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to output file \"{1}\" does not exist.", encoderItem.InputFileName, encoderItem.OutputFileName);
                        encoderItem.Status = EncoderItemStatus.Failed;
                        encoderItem.AddError(string.Format("Input file \"{0}\" does not exist.", encoderItem.OutputFileName));
                        return;
                    }
                    if (!this.CheckOutput(encoderItem.OutputFileName))
                    {
                        Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to output file \"{1}\" already exists or cannot be written.", encoderItem.InputFileName, encoderItem.OutputFileName);
                        encoderItem.Status = EncoderItemStatus.Failed;
                        encoderItem.AddError(string.Format("Output file \"{0}\" already exists or cannot be written.", encoderItem.OutputFileName));
                        return;
                    }
                    Logger.Write(this, LogLevel.Debug, "Beginning encoding file \"{0}\" to output file \"{1}\".", encoderItem.InputFileName, encoderItem.OutputFileName);
                    encoderItem.Progress = EncoderItem.PROGRESS_NONE;
                    encoderItem.Status = EncoderItemStatus.Processing;
                    this.Encode(encoderItem, settings);
                    if (encoderItem.Status == EncoderItemStatus.Complete)
                    {
                        Logger.Write(this, LogLevel.Debug, "Encoding file \"{0}\" to output file \"{1}\" completed successfully.", encoderItem.InputFileName, encoderItem.OutputFileName);
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Warn, "Encoding file \"{0}\" failed: Unknown error.", encoderItem.InputFileName);
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", encoderItem.InputFileName);
                    encoderItem.Status = EncoderItemStatus.Cancelled;
                    return;
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Encoding file \"{0}\" failed: {1}", encoderItem.InputFileName, e.Message);
                    encoderItem.Status = EncoderItemStatus.Failed;
                    encoderItem.AddError(e.Message);
                }
                finally
                {
                    encoderItem.Progress = EncoderItem.PROGRESS_COMPLETE;
                }
            });
            Logger.Write(this, LogLevel.Debug, "Encoding completed successfully.");
        }

        protected virtual void Encode(EncoderItem encoderItem, IBassEncoderSettings settings)
        {
            var flags = BassFlags.Decode;
            if (this.ShouldDecodeFloat(encoderItem, settings))
            {
                Logger.Write(this, LogLevel.Debug, "Decoding file \"{0}\" in high quality mode (32 bit floating point).", encoderItem.InputFileName);
                flags |= BassFlags.Float;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Decoding file \"{0}\" in standaed quality mode (16 bit integer).", encoderItem.InputFileName);
            }
            var stream = this.CreateStream(encoderItem.InputFileName, flags);
            if (stream.IsEmpty)
            {
                Logger.Write(this, LogLevel.Debug, "Failed to create stream for file \"{0}\": Unknown error.", encoderItem.InputFileName);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Created stream for file \"{0}\": {1}", encoderItem.InputFileName, stream.ChannelHandle);
            try
            {
                if (flags.HasFlag(BassFlags.Float))
                {
                    this.EncodeWithResampler(encoderItem, stream, settings);
                }
                else
                {
                    this.Encode(encoderItem, stream, settings);
                }
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "Releasing stream for file \"{0}\": {1}", encoderItem.InputFileName, stream.ChannelHandle);
                Bass.StreamFree(stream.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
            }
            if (this.CancellationToken.IsCancellationRequested)
            {
                encoderItem.Status = EncoderItemStatus.Cancelled;
                if (File.Exists(encoderItem.OutputFileName))
                {
                    Logger.Write(this, LogLevel.Debug, "Deleting incomplete output \"{0}\": Cancelled.", encoderItem.OutputFileName);
                    File.Delete(encoderItem.OutputFileName);
                }
            }
            else
            {
                encoderItem.Status = EncoderItemStatus.Complete;
            }
        }

        protected virtual void Encode(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            using (var encoderProcess = this.CreateEncoderProcess(encoderItem, stream, settings))
            {
                this.Encode(encoderItem, stream, encoderProcess);
                if (this.WaitForExit(encoderProcess))
                {
                    if (encoderProcess.ExitCode != 0)
                    {
                        throw new InvalidOperationException("Process does not indicate success.");
                    }
                }
            }
        }

        protected virtual void Encode(EncoderItem encoderItem, IBassStream stream, Process encoderProcess)
        {
            var channelReader = new ChannelReader(encoderItem, stream);
            var encoderWriter = new ProcessWriter(encoderProcess);
            var thread = new Thread(() =>
            {
                this.Try(() => channelReader.CopyTo(encoderWriter, this.CancellationToken), this.GetErrorHandler(encoderItem));
            })
            {
                Name = string.Format("ChannelReader(\"{0}\", {1})", encoderItem.InputFileName, stream.ChannelHandle),
                IsBackground = true
            };
            Logger.Write(this, LogLevel.Debug, "Starting background thread for file \"{0}\".", encoderItem.InputFileName);
            thread.Start();
            Logger.Write(this, LogLevel.Debug, "Completing background thread for file \"{0}\".", encoderItem.InputFileName);
            this.Join(thread);
            encoderWriter.Close();
        }

        protected virtual void EncodeWithResampler(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            using (var resamplerProcess = this.CreateResamplerProcess(encoderItem, stream, new SoxEncoderSettings(settings)))
            {
                using (var encoderProcess = this.CreateEncoderProcess(encoderItem, stream, settings))
                {
                    var success = true;
                    this.EncodeWithResampler(encoderItem, stream, resamplerProcess, encoderProcess);
                    if (this.WaitForExit(resamplerProcess))
                    {
                        if (resamplerProcess.ExitCode != 0)
                        {
                            success = false;
                        }
                    }
                    if (this.WaitForExit(encoderProcess))
                    {
                        if (encoderProcess.ExitCode != 0)
                        {
                            success = false;
                        }
                    }
                    if (!success)
                    {
                        throw new InvalidOperationException("Process does not indicate success.");
                    }
                }
            }
        }

        protected virtual void EncodeWithResampler(EncoderItem encoderItem, IBassStream stream, Process resamplerProcess, Process encoderProcess)
        {
            var channelReader = new ChannelReader(encoderItem, stream);
            var resamplerReader = new ProcessReader(resamplerProcess);
            var resamplerWriter = new ProcessWriter(resamplerProcess);
            var encoderWriter = new ProcessWriter(encoderProcess);
            var threads = new[]
            {
                new Thread(() =>
                {
                    this.Try(() => channelReader.CopyTo(resamplerWriter,this.CancellationToken), this.GetErrorHandler(encoderItem));
                })
                {
                    Name = string.Format("ChannelReader(\"{0}\", {1})", encoderItem.InputFileName, stream.ChannelHandle),
                    IsBackground = true
                },
                new Thread(() =>
                {
                    this.Try(() => resamplerReader.CopyTo(encoderWriter,this.CancellationToken), this.GetErrorHandler(encoderItem));
                })
                {
                    Name = string.Format("ProcessReader(\"{0}\", {1})", encoderItem.InputFileName, resamplerProcess.Id),
                    IsBackground = true
                }
            };
            Logger.Write(this, LogLevel.Debug, "Starting background threads for file \"{0}\".", encoderItem.InputFileName);
            foreach (var thread in threads)
            {
                thread.Start();
            }
            Logger.Write(this, LogLevel.Debug, "Completing background threads for file \"{0}\".", encoderItem.InputFileName);
            foreach (var thread in threads)
            {
                this.Join(thread);
            }
            resamplerReader.Close();
            resamplerWriter.Close();
            encoderWriter.Close();
        }

        public void Update()
        {
            //Nothing to do.
        }

        public void Cancel()
        {
            if (this.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            Logger.Write(this, LogLevel.Warn, "Cancellation requested, shutting down.");
            this.CancellationToken.Cancel();
        }

        protected virtual bool CheckInput(string fileName)
        {
            if (!string.IsNullOrEmpty(Path.GetPathRoot(fileName)) && !File.Exists(fileName))
            {
                //TODO: Bad .Result
                if (!NetworkDrive.IsRemotePath(fileName) || !NetworkDrive.ConnectRemotePath(fileName).Result)
                {
                    throw new FileNotFoundException(string.Format("File not found: {0}", fileName), fileName);
                }
            }
            return true;
        }

        protected virtual bool CheckOutput(string fileName)
        {
            var directoryName = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(Path.GetPathRoot(directoryName)) && !Directory.Exists(directoryName))
            {
                //TODO: Bad .Result
                if (!NetworkDrive.IsRemotePath(directoryName) || !NetworkDrive.ConnectRemotePath(directoryName).Result)
                {
                    try
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    catch
                    {
                        throw new DirectoryNotFoundException(string.Format("Directory not found: {0}", directoryName));
                    }
                }
            }
            if (File.Exists(fileName))
            {
                return false;
            }
            return true;
        }

        protected virtual IBassStream CreateStream(string fileName, BassFlags flags)
        {
            const int INTERVAL = 5000;
            var streamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            var playlistItem = new PlaylistItem()
            {
                FileName = fileName
            };
        retry:
            if (this.CancellationToken.IsCancellationRequested)
            {
                return BassStream.Empty;
            }
            var stream = streamFactory.CreateBasicStream(
                playlistItem,
                flags
            );
            if (stream.IsEmpty)
            {
                if (stream.Errors == Errors.Already)
                {
                    Logger.Write(this, LogLevel.Trace, "Failed to create decoder stream for file \"{0}\": Device is already in use.", fileName);
                    Thread.Sleep(INTERVAL);
                    goto retry;
                }
                throw new InvalidOperationException(string.Format("Failed to create decoder stream for file \"{0}\": {1}", fileName, Enum.GetName(typeof(Errors), stream.Errors)));
            }
            return stream;
        }

        protected virtual Process CreateResamplerProcess(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            Logger.Write(this, LogLevel.Debug, "Creating resampler process for file \"{0}\".", encoderItem.InputFileName);
            var arguments = settings.GetArguments(encoderItem, stream);
            var process = this.CreateProcess(
                encoderItem,
                stream,
                settings.Executable,
                settings.Directory,
                arguments,
                true,
                true,
                true
            );
            Logger.Write(this, LogLevel.Debug, "Created resampler process for file \"{0}\": \"{1}\" {2}", encoderItem.InputFileName, settings.Executable, arguments);
            return process;
        }

        protected virtual Process CreateEncoderProcess(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            Logger.Write(this, LogLevel.Debug, "Creating encoder process for file \"{0}\".", encoderItem.InputFileName);
            var arguments = settings.GetArguments(encoderItem, stream);
            var process = this.CreateProcess(
                encoderItem,
                stream,
                settings.Executable,
                settings.Directory,
                arguments,
                true,
                false,
                true
            );
            Logger.Write(this, LogLevel.Debug, "Created encoder process for file \"{0}\": \"{1}\" {2}", encoderItem.InputFileName, settings.Executable, arguments);
            return process;
        }

        protected virtual Process CreateProcess(EncoderItem encoderItem, IBassStream stream, string executable, string directory, string arguments, bool redirectStandardInput, bool redirectStandardOutput, bool redirectStandardError)
        {
            if (!File.Exists(executable))
            {
                throw new InvalidOperationException(string.Format("A required utility was not found: {0}", executable));
            }
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = executable,
                WorkingDirectory = directory,
                Arguments = arguments,
                RedirectStandardInput = redirectStandardInput,
                RedirectStandardOutput = redirectStandardOutput,
                RedirectStandardError = redirectStandardError,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            try
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch
            {
                //Nothing can be done, probably access denied.
            }
            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }
                Logger.Write(this, LogLevel.Trace, "{0}: {1}", executable, e.Data);
                encoderItem.AddError(e.Data);
            };
            process.BeginErrorReadLine();
            return process;
        }

        protected virtual bool ShouldDecodeFloat(EncoderItem encoderItem, IBassEncoderSettings settings)
        {
            if (encoderItem.BitsPerSample == 1)
            {
                Logger.Write(this, LogLevel.Debug, "Suggesting high quality mode for file \"{0}\": dsd.", encoderItem.InputFileName);
                return true;
            }
            if (encoderItem.BitsPerSample > 16 || settings.Format.Depth > 16)
            {
                Logger.Write(this, LogLevel.Debug, "Suggesting high quality mode for file \"{0}\": >16 bit.", encoderItem.InputFileName);
                return true;
            }
            Logger.Write(this, LogLevel.Debug, "Suggesting standard quality mode for file \"{0}\": <=16 bit.", encoderItem.InputFileName);
            return false;
        }

        protected virtual Action<Exception> GetErrorHandler(EncoderItem encoderItem)
        {
            return e =>
            {
                Logger.Write(this, LogLevel.Warn, "Encoder background thread for file \"{0}\" error: {1}", encoderItem.InputFileName, e.Message);
                encoderItem.AddError(e.Message);
            };
        }

        protected virtual bool Join(Thread thread)
        {
            const int INTERVAL = 5000;
            while (thread.IsAlive)
            {
                if (thread.Join(INTERVAL))
                {
                    break;
                }
                if (this.CancellationToken.IsCancellationRequested)
                {
                    if (thread.Join(INTERVAL))
                    {
                        break;
                    }
                    thread.Abort();
                    return false;
                }
            }
            return true;
        }

        protected virtual bool WaitForExit(Process process)
        {
            const int INTERVAL = 5000;
            while (!process.HasExited)
            {
                if (process.WaitForExit(INTERVAL))
                {
                    break;
                }
                if (this.CancellationToken.IsCancellationRequested)
                {
                    if (process.WaitForExit(INTERVAL))
                    {
                        break;
                    }
                    process.Kill();
                    return false;
                }
            }
            return true;
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            //Nothing to do.
        }

        ~BassEncoder()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
