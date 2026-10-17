#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.DeviceDownloaders/DeviceDownloader.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Services.DeviceDownloaders
{
    ///<inheritdoc cref="IDeviceDownloader"/>
    public class DeviceDownloader : ExecutableServiceWithProgressAsyncBase<Device, Tuple<Device, FileInfo>, ControllerDownloadProgress>, IDeviceDownloader
    {
        #region Fields

        private readonly IEnumerable<IDownloaderClient> _clients;
        protected readonly ILogger _log;
        protected readonly DeviceDownloaderConfiguration _options;

        #endregion

        ///<inheritdoc/>
        public DeviceDownloader(IEnumerable<IDownloaderClient> clients, ILogger<IDeviceDownloader> log, IOptionsSnapshot<DeviceDownloaderConfiguration> options) : base(true)
        {
            _clients = clients;
            _log = log;
            _options = options?.Get(GetType().Name) ?? options?.Value;
        }

        #region Methods

        //public override void Initialize()
        //{
        //}

        /// <summary>
        /// Generates a <see cref="Uri"/> resource used for temp event log storage
        /// </summary>
        /// <param name="device"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual Uri GenerateLocalFilePath(Device device, Uri resource)
        {
            var fileExtension = Path.HasExtension(resource.Segments.LastOrDefault()) ? Path.GetExtension(resource.Segments.LastOrDefault()) : ".txt";
            var fileName = $"{device.DeviceIdentifier}-{DateTime.Now.Ticks}{fileExtension}";

            var path = Path.Combine
                (_options.BasePath,
                $"{device.Location?.LocationIdentifier}",
                device.DeviceType.ToString(),
                 $"{device.DeviceIdentifier}-{device.Ipaddress}");

            var result = new UriBuilder()
            {
                Scheme = Uri.UriSchemeFile,
                Path = Path.Combine(path, fileName)
            }.Uri;

            return result;
        }

        ///<inheritdoc/>
        public override bool CanExecute(Device value)
        {
            return value.LoggingEnabled && value?.DeviceConfiguration?.Protocol != TransportProtocols.Unknown;
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidDeviceIpAddressException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public override async IAsyncEnumerable<Tuple<Device, FileInfo>> Execute(Device parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"Parameter can not be null");

            var client = _clients.FirstOrDefault(w => parameter.DeviceConfiguration.Protocol == w.Protocol);

            if (CanExecute(parameter))
            {
                var logMessages = new DeviceDownloaderLogMessages(_log, this.GetType().Name, parameter);

                if (!IPAddress.TryParse(parameter?.Ipaddress, out IPAddress ipaddress) || ipaddress == null || !ipaddress.IsValidIpAddress())
                {
                    var exception = new InvalidDeviceIpAddressException(parameter);
                    logMessages.InvalidDeviceIpAddressException(ipaddress, exception);
                    throw exception;
                }
                else if (_options.Ping && !await ipaddress.PingIpAddressAsync())
                {
                    var exception = new InvalidDeviceIpAddressException(parameter);
                    logMessages.InvalidDeviceIpAddressException(ipaddress, exception);
                    throw exception;
                }

                var deviceIdentifier = parameter?.DeviceIdentifier;
                var user = parameter?.DeviceConfiguration?.UserName;
                var password = parameter?.DeviceConfiguration?.Password;
                var path = new ObjectPropertyParser(parameter, parameter?.DeviceConfiguration?.Path).ToString();
                var query = parameter?.DeviceConfiguration?.Query.Select(s => new ObjectPropertyParser(parameter, s).ToString()).ToArray();
                var connectionTimeout = parameter?.DeviceConfiguration?.ConnectionTimeout ?? 2000;
                var operationTimeout = parameter?.DeviceConfiguration?.OperationTimeout ?? 2000;
                var props = parameter?.DeviceConfiguration?.ConnectionProperties?.ToDictionary(k => k.Key, k => k.Value.ToString());

                using (client)
                {
                    try
                    {
                        var connection = new IPEndPoint(ipaddress, parameter.DeviceConfiguration.Port);
                        var credentials = new NetworkCredential(user, password, ipaddress.ToString());

                        logMessages.ConnectingToHostMessage(deviceIdentifier, ipaddress);

                        await client.ConnectAsync(connection, credentials, connectionTimeout, operationTimeout, props, cancelToken);
                    }
                    catch (DownloaderClientConnectionException e)
                    {
                        logMessages.ConnectingToHostException(deviceIdentifier, ipaddress, e);
                    }
                    catch (OperationCanceledException e)
                    {
                        logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                    }

                    if (client.IsConnected)
                    {
                        logMessages.ConnectedToHostMessage(deviceIdentifier, ipaddress);

                        IEnumerable<Uri> resources = new List<Uri>();

                        try
                        {
                            logMessages.GettingsResourcesListMessage(deviceIdentifier, ipaddress, path);

                            resources = await client.ListResourcesAsync(path, cancelToken, query);
                        }
                        catch (DownloaderClientListResourcesException e)
                        {
                            logMessages.ResourceListingException(deviceIdentifier, ipaddress, path, e);
                        }
                        catch (DownloaderClientConnectionException e)
                        {
                            logMessages.NotConnectedToHostException(deviceIdentifier, ipaddress, e);
                        }

                        if (resources == null) yield break;

                        int total = resources.Count();
                        int current = 0;

                        logMessages.ResourceListingMessage(total, deviceIdentifier, ipaddress);

                        foreach (var resource in resources)
                        {
                            var locatlPath = GenerateLocalFilePath(parameter, resource);
                            FileInfo downloadedFile = null;

                            try
                            {
                                logMessages.DownloadingResourceMessage(resource, deviceIdentifier, ipaddress);

                                downloadedFile = await client.DownloadResourceAsync(locatlPath, resource, cancelToken);
                            }
                            catch (DownloaderClientDownloadResourceException e)
                            {
                                logMessages.DownloadResourceException(resource, deviceIdentifier, ipaddress, e);
                            }
                            catch (DownloaderClientConnectionException e)
                            {
                                logMessages.NotConnectedToHostException(deviceIdentifier, ipaddress, e);
                            }
                            catch (OperationCanceledException e)
                            {
                                logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                            }

                            if (downloadedFile == null) continue;

                            current++;

                            logMessages.DownloadedResourceMessage(resource, deviceIdentifier, ipaddress);

                            progress?.Report(new ControllerDownloadProgress(downloadedFile, current, total));

                            yield return Tuple.Create(parameter, downloadedFile);

                            if (_options.DeleteRemoteFile)
                            {
                                try
                                {
                                    logMessages.DeletingResourceMessage(resource, deviceIdentifier, ipaddress);

                                    await client.DeleteResourceAsync(resource, cancelToken);
                                }
                                catch (DownloaderClientDeleteResourceException e)
                                {
                                    logMessages.DeleteResourceException(resource, deviceIdentifier, ipaddress, e);
                                }
                                catch (DownloaderClientConnectionException e)
                                {
                                    logMessages.NotConnectedToHostException(deviceIdentifier, ipaddress, e);
                                }
                                catch (OperationCanceledException e)
                                {
                                    logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                                }

                                logMessages.DeletedResourceMessage(resource, deviceIdentifier, ipaddress);
                            }
                        }

                        logMessages.DownloadedResourcesMessage(current, total, deviceIdentifier, ipaddress);

                        try
                        {
                            logMessages.DisconnectingFromHostMessage(deviceIdentifier, ipaddress);

                            await client.DisconnectAsync(cancelToken);
                        }
                        catch (DownloaderClientConnectionException e)
                        {
                            logMessages.DisconnectingFromHostException(deviceIdentifier, ipaddress, e);
                        }
                        catch (OperationCanceledException e)
                        {
                            logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                        }
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
            else
            {
                throw new ExecuteException();
            }
        }

        #endregion

        ///<inheritdoc/>
        protected override void DisposeManagedCode()
        {
            foreach (var client in _clients)
            {
                client.Dispose();
            }
        }
    }

    /// <summary>
    /// Takes a string and parses against the properties of the provided object
    /// </summary>
    public class ObjectPropertyParser
    {
        private readonly object _obj;
        private readonly string _value;

        /// <summary>
        /// Takes <paramref name="value"/> and parses against the properties of the provided <paramref name="obj"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public ObjectPropertyParser(object obj, string value)
        {
            _obj = obj;
            _value = value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var i in _value.Split('[', ']').ToArray())
            {
                if (i.StartsWith("DateTime"))
                {
                    builder.AppendFormat("{0" + i.Replace("DateTime", "") + "}", DateTime.Now);
                }

                else if (i.StartsWith("LogStartTime"))
                {
                    var localTime = DateTime.Now;

                    if (_obj.ExtractInterface(out ICoordinates c))
                    {
                        localTime = c.GetLocalTimeFromCoordinates();
                    }

                    if (_obj is Device d && d.DeviceConfiguration != null)
                    {
                        builder.AppendFormat("{0" + i.Replace("LogStartTime", "") + "}", localTime.AddMinutes(-d.DeviceConfiguration.LoggingOffset));
                    }
                }

                else if (i.StartsWith(_obj.GetType().Name))
                {
                    builder.AppendFormat(new ObjectPropertiesFormatProvider(), "{0" + i.Remove(0, _obj.GetType().Name.Length) + "}", _obj);
                }

                else
                {
                    builder.Append(i);
                }
            }

            return builder.ToString();
        }
    }
}
