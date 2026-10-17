#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients/DownloaderClientBase.cs
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

using System.Net;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients
{
    ///<inheritdoc/>
    public abstract class DownloaderClientBase : ServiceObjectBase, IDownloaderClient
    {
        ///<inheritdoc/>
        public DownloaderClientBase() : base(true) { }

        #region IDownloaderClient

        ///<inheritdoc/>
        public abstract TransportProtocols Protocol { get; }

        ///<inheritdoc/>
        public abstract bool IsConnected { get; }

        ///<inheritdoc/>
        public async Task ConnectAsync(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTimeout = 2000, Dictionary<string, string> connectionProperties = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                ArgumentNullException.ThrowIfNull(connection);

                ArgumentNullException.ThrowIfNull(credentials);

                await Connect(connection, credentials, connectionTimeout, operationTimeout, connectionProperties, token);
            }
            catch (Exception e)
            {
                throw new DownloaderClientConnectionException(credentials?.Domain, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task DeleteResourceAsync(Uri resource, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(null, this, "Client not connected");

            try
            {
                ArgumentNullException.ThrowIfNull(resource);

                await DeleteResource(resource, token);
            }
            catch (Exception e)
            {
                throw new DownloaderClientDeleteResourceException(resource, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(null, this, "Client not connected");

            try
            {
                await Disconnect(token);
            }
            catch (Exception e)
            {
                throw new DownloaderClientConnectionException(null, this, e.Message, e);
            }

            await Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task<FileInfo> DownloadResourceAsync(Uri local, Uri remote, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(null, this, "Client not connected");

            try
            {
                ArgumentNullException.ThrowIfNull(local);

                ArgumentNullException.ThrowIfNull(remote);

                if (!local.IsFile || !local.IsAbsoluteUri)
                    throw new UriFormatException($"Invalid Uri {nameof(local)} needs to be of scheme type {Uri.UriSchemeFile}");

                if (TryCreateFileInfo(local, out FileInfo file))
                {
                    file.Directory.Create();

                    return await DownloadResource(file, remote, token);
                }
                else
                    throw new FileNotFoundException(local.LocalPath);
            }
            catch (Exception e)
            {
                throw new DownloaderClientDownloadResourceException(remote, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<Uri>> ListResourcesAsync(string path, CancellationToken token = default, params string[] query)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(null, this, "Client not connected");

            try
            {
                return await ListResources(path, token, query);
            }
            catch (Exception e)
            {
                throw new DownloaderClientListResourcesException(path, this, e.Message);
            }
        }

        #endregion

        /// <summary>
        /// Tries and creates a <see cref="FileInfo"/> instance from a valid <see cref="Uri"/> with a <see cref="Uri.UriSchemeFile"/> schema
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        protected bool TryCreateFileInfo(Uri uri, out FileInfo file)
        {
            if (uri.IsFile && uri.IsUnc && uri.Host == "localhost")
            {
                var path = uri.LocalPath.Replace("\\\\localhost\\", "");

                var fileCheck = !Path.GetFileName(path).Any(a => Path.GetInvalidFileNameChars().Contains(a));
                var extCheck = Path.HasExtension(path);
                if (fileCheck && extCheck)
                {
                    file = new FileInfo(path);
                    return true;
                }
            }

            file = null;
            return false;
        }

        /// <inheritdoc cref="IDownloaderClient.ConnectAsync(IPEndPoint, NetworkCredential, int, int, Dictionary{string, string}, CancellationToken)"/>
        protected abstract Task Connect(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTimeout = 2000, Dictionary<string, string> connectionProperties = null, CancellationToken token = default);

        /// <inheritdoc cref="IDownloaderClient.DeleteResourceAsync(Uri, CancellationToken)"></inheritdoc>
        protected abstract Task DeleteResource(Uri resource, CancellationToken token = default);

        /// <inheritdoc cref="IDownloaderClient.DisconnectAsync(CancellationToken)"></inheritdoc>
        protected abstract Task Disconnect(CancellationToken token = default);

        /// <inheritdoc cref="IDownloaderClient.DownloadResourceAsync(Uri, Uri, CancellationToken)"></inheritdoc>
        protected abstract Task<FileInfo> DownloadResource(FileInfo file, Uri remote, CancellationToken token = default);

        /// <inheritdoc cref="IDownloaderClient.ListResourcesAsync(string, CancellationToken, string[])"></inheritdoc>
        protected abstract Task<IEnumerable<Uri>> ListResources(string path, CancellationToken token = default, params string[] query);
    }
}
